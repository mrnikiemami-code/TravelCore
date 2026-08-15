import {
  getHtmlDir,
  getHtmlLang,
  isAppLocale,
  type AppLocale,
} from "@/lib/i18n";
import { notFound } from "next/navigation";

/**
 * Minimal locale home — routing/root proof + semantic token smoke (not a product page).
 */
export default async function LocaleHomePage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const lang = getHtmlLang(locale);
  const dir = getHtmlDir(locale);

  return (
    <main className="flex flex-1 flex-col items-center justify-center gap-4 bg-background p-8 text-foreground">
      <div className="w-full max-w-content rounded-lg border border-border bg-surface p-6 shadow-sm">
        <h1 className="text-heading font-semibold tracking-tight">TravelCore</h1>
        <p className="mt-2 text-body text-muted-foreground">
          Locale foundation — <code className="text-label">{locale}</code>
        </p>
        <p className="mt-1 text-caption text-muted-foreground">
          document: lang=<code>{lang}</code> dir=<code>{dir}</code>
        </p>
        <div className="mt-6 flex flex-wrap gap-3">
          <span className="inline-flex min-h-touch items-center rounded-md bg-primary px-4 text-label text-primary-foreground">
            primary
          </span>
          <span className="inline-flex min-h-touch items-center rounded-md bg-surface-muted px-4 text-label text-foreground ring-2 ring-focus">
            focus ring
          </span>
          <span className="inline-flex min-h-touch items-center rounded-md bg-danger px-4 text-label text-danger-foreground">
            danger
          </span>
        </div>
      </div>
    </main>
  );
}
