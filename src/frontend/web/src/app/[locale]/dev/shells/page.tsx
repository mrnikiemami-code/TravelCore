import { notFound } from "next/navigation";
import {
  AdminShell,
  AgencyShell,
  PublicFooter,
  PublicHeader,
  PublicShell,
} from "@/components/shell";
import { Surface, Text } from "@/components/ui";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata = {
  robots: { index: false, follow: false },
  title: "P30 Shell Board",
};

/**
 * Visual board for TC-P30-T004 architect review.
 * Not a product commerce page.
 */
export default async function ShellBoardPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  return (
    <div className="space-y-10 bg-surface-muted p-4 md:p-8">
      <header className="mx-auto max-w-wide space-y-2">
        <Text as="h1" role="heading">
          P30 Application Shells — Visual Board
        </Text>
        <Text as="p" role="body" className="text-muted-foreground">
          TC-P30-T004 · Public / Admin / Agency · North Star aligned token
          candidates · not a sellable Home page (that is T005).
        </Text>
      </header>

      <section className="mx-auto max-w-wide space-y-3" aria-labelledby="public-shell">
        <h2 id="public-shell" className="text-sm font-semibold uppercase tracking-wide text-primary">
          Public shell
        </h2>
        <Surface className="overflow-hidden p-0 shadow-md">
          <PublicShell
            embedded
            header={<PublicHeader locale={locale} />}
            footer={<PublicFooter locale={locale} />}
          >
            <div className="space-y-4 p-6">
              <p className="text-2xl font-semibold text-foreground">
                Discover + Trust + Book
              </p>
              <p className="max-w-2xl text-muted-foreground">
                Shell chrome only — product home sections remain T005.
              </p>
              <div className="inline-flex rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground">
                Primary CTA sample
              </div>
              <div className="ms-2 inline-flex rounded-md bg-accent px-4 py-2 text-sm font-medium text-accent-foreground">
                Accent CTA sample
              </div>
            </div>
          </PublicShell>
        </Surface>
      </section>

      <section className="mx-auto max-w-wide space-y-3" aria-labelledby="admin-shell">
        <h2 id="admin-shell" className="text-sm font-semibold uppercase tracking-wide text-primary">
          Admin shell
        </h2>
        <Surface className="overflow-hidden p-0 shadow-md">
          <AdminShell
            embedded
            header={<span>Admin Console</span>}
            breadcrumb={<span>Catalog / Overview</span>}
            navigation={
              <ul className="flex flex-col gap-1 text-sm">
                <li className="rounded-md bg-surface-muted px-2 py-2 font-medium">Catalog</li>
                <li className="rounded-md px-2 py-2 hover:bg-surface-muted">Media</li>
                <li className="rounded-md px-2 py-2 hover:bg-surface-muted">UGC</li>
              </ul>
            }
            actions={
              <button
                type="button"
                className="min-h-touch rounded-md bg-primary px-3 text-sm text-primary-foreground"
              >
                New
              </button>
            }
          >
            <div className="p-6 text-sm text-muted-foreground">
              Dense operational workspace frame — grids land in later Admin tasks.
            </div>
          </AdminShell>
        </Surface>
      </section>

      <section className="mx-auto max-w-wide space-y-3" aria-labelledby="agency-shell">
        <h2 id="agency-shell" className="text-sm font-semibold uppercase tracking-wide text-primary">
          Agency shell
        </h2>
        <Surface className="overflow-hidden p-0 shadow-md">
          <AgencyShell embedded locale={locale} title="Sales dashboard">
            <div className="grid gap-3 p-4 sm:grid-cols-3">
              <Surface className="p-4">
                <p className="text-xs text-muted-foreground">Offers</p>
                <p className="text-lg font-semibold">—</p>
              </Surface>
              <Surface className="p-4">
                <p className="text-xs text-muted-foreground">Publish queue</p>
                <p className="text-lg font-semibold">—</p>
              </Surface>
              <Surface className="p-4">
                <p className="text-xs text-muted-foreground">Partner trust</p>
                <p className="text-lg font-semibold text-accent">Ready</p>
              </Surface>
            </div>
          </AgencyShell>
        </Surface>
      </section>
    </div>
  );
}
