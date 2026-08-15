import { NotFoundView } from "@/components/ui/not-found-view";
import { DEFAULT_LOCALE } from "@/lib/i18n";

/**
 * Global not-found (e.g. invalid locale segment).
 * Uses product default locale copy; valid-locale misses use `[locale]/not-found.tsx`.
 */
export default function GlobalNotFound() {
  return (
    <html lang={DEFAULT_LOCALE} dir="rtl">
      <body className="min-h-full bg-background text-foreground antialiased">
        <NotFoundView locale={DEFAULT_LOCALE} />
      </body>
    </html>
  );
}
