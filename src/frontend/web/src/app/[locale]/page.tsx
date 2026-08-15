import {
  getHtmlDir,
  getHtmlLang,
  isAppLocale,
  type AppLocale,
} from "@/lib/i18n";
import { notFound } from "next/navigation";

/**
 * Minimal locale home — routing/root proof only (not a product page).
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
    <main className="flex flex-1 flex-col items-center justify-center gap-4 p-8">
      <h1 className="text-2xl font-semibold tracking-tight">TravelCore</h1>
      <p className="text-zinc-600 dark:text-zinc-400">
        Locale foundation — <code>{locale}</code>
      </p>
      <p className="text-sm text-zinc-500">
        document: lang=<code>{lang}</code> dir=<code>{dir}</code>
      </p>
    </main>
  );
}
