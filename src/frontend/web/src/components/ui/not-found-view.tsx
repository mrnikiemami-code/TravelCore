import type { AppLocale } from "@/lib/i18n";
import { RouteStatePanel } from "@/components/ui/route-state";
import { getNotFoundCopy } from "@/lib/i18n/ui-labels";

export function NotFoundView({ locale }: { locale: AppLocale }) {
  const copy = getNotFoundCopy(locale);

  return (
    <main id="main-content" tabIndex={-1} className="outline-none">
      <RouteStatePanel
        title={copy.title}
        body={copy.body}
        actions={
          <a
            href={`/${locale}`}
            className="inline-flex min-h-touch items-center justify-center rounded-md bg-primary px-4 text-label text-primary-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-focus focus-visible:ring-offset-2 focus-visible:ring-offset-background"
          >
            {copy.home}
          </a>
        }
      />
    </main>
  );
}
