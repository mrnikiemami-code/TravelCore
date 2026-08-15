import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Geist, Geist_Mono } from "next/font/google";
import { SkipLink } from "@/components/ui/skip-link";
import {
  getHtmlDir,
  getHtmlLang,
  isAppLocale,
  listPublicLocales,
  type AppLocale,
} from "@/lib/i18n";
import "../globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const dynamicParams = false;

export function generateStaticParams() {
  return listPublicLocales().map((locale) => ({ locale }));
}

export async function generateMetadata({
  params,
}: {
  params: Promise<{ locale: string }>;
}): Promise<Metadata> {
  const { locale } = await params;
  if (!isAppLocale(locale)) {
    return { title: "TravelCore" };
  }
  // Minimal locale-aware metadata only — full hreflang/canonical → later SEO tasks.
  return {
    title: "TravelCore",
    description: "TravelCore public web foundation",
  };
}

function skipLinkLabel(locale: AppLocale): string {
  if (locale === "fa") return "پرش به محتوا";
  if (locale === "ar") return "تخطى إلى المحتوى";
  return "Skip to content";
}

export default async function LocaleLayout({
  children,
  params,
}: Readonly<{
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
}>) {
  const { locale: localeParam } = await params;

  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const lang = getHtmlLang(locale);
  const dir = getHtmlDir(locale);

  return (
    <html
      lang={lang}
      dir={dir}
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col">
        <SkipLink hrefId="main-content">{skipLinkLabel(locale)}</SkipLink>
        {children}
      </body>
    </html>
  );
}
