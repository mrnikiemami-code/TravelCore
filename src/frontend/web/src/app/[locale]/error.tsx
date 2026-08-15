"use client";

import { useParams } from "next/navigation";
import { RouteStatePanel } from "@/components/ui/route-state";
import { getErrorCopy, normalizeUiLocale } from "@/lib/i18n/ui-labels";

type ErrorProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

/**
 * App Router error boundary — Client Component required by Next.js contract.
 * Does NOT render error.message / stack / internals to the user.
 */
export default function LocaleError({ error, reset }: ErrorProps) {
  const params = useParams();
  const locale = normalizeUiLocale(
    typeof params?.locale === "string" ? params.locale : undefined,
  );
  const copy = getErrorCopy(locale);

  // Keep digest available for observability hooks later — never display it.
  void error.digest;

  return (
    <main id="main-content" tabIndex={-1} className="outline-none">
      <RouteStatePanel
        title={copy.title}
        body={copy.body}
        actions={
          <div className="flex flex-wrap gap-3">
            <button
              type="button"
              onClick={() => reset()}
              className="inline-flex min-h-touch items-center justify-center rounded-md bg-primary px-4 text-label text-primary-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-focus focus-visible:ring-offset-2 focus-visible:ring-offset-background"
            >
              {copy.retry}
            </button>
            <a
              href={`/${locale}`}
              className="inline-flex min-h-touch items-center justify-center rounded-md border border-border bg-surface px-4 text-label text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-focus focus-visible:ring-offset-2 focus-visible:ring-offset-background"
            >
              {copy.home}
            </a>
          </div>
        }
      />
    </main>
  );
}
