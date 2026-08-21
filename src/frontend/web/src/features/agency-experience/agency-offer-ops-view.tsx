"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { FieldMessage, LtrValue, Surface, Text } from "@/components/ui";
import {
  createAgencyOfferAction,
  mutateAgencyOfferLifecycleAction,
  type AgencyOfferLifecycleAction,
  type AgencyOfferPanelItem,
  type AgencyProfilePanel,
} from "@/features/agency-experience/agency-offer-ops-actions";
import type { AppLocale } from "@/lib/i18n";

const fieldClass =
  "min-h-touch rounded-lg border border-border bg-background px-3 py-2 outline-none ring-accent focus:ring-2";

/**
 * Agency Offer Operations foundation (TC-P38-T007).
 * Acting-agency offers only · no Commission/Settlement/fake metrics.
 */
export function AgencyOfferOpsListView({
  locale,
  profile,
  items,
  loadError,
}: {
  locale: AppLocale;
  profile: AgencyProfilePanel | null;
  items: AgencyOfferPanelItem[];
  loadError: string | null;
}) {
  const router = useRouter();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [tourProductId, setTourProductId] = useState("");
  const [titleOverride, setTitleOverride] = useState("");

  const copy =
    locale === "fa"
      ? {
          eyebrow: "عملیات Offer آژانس",
          title: "کاتالوگ فروش · Agency Offers",
          intro:
            "لیست و چرخهٔ عمر Offerهای همین آژانس — نه Offer آژانس دیگر، نه کمیسیون جعلی.",
          profileMissing:
            "پروفایل آژانس برای حساب فعلی پیدا نشد. ابتدا AgencyProfile را بسازید/فعال کنید.",
          empty: "هنوز Offerی برای این آژانس ثبت نشده است.",
          create: "ایجاد Offer",
          tourProductId: "شناسه TourProduct",
          titleOverride: "عنوان جایگزین (اختیاری)",
          back: "بازگشت به داشبورد",
          boundary: "Agency A نمی‌تواند Offerهای Agency B را مدیریت کند · Commission = deferred",
        }
      : {
          eyebrow: "Agency Offer ops",
          title: "Sellable catalog · Agency Offers",
          intro:
            "List and lifecycle for this agency’s offers only — not peer agencies, not fake commission.",
          profileMissing:
            "No AgencyProfile for the acting account yet. Create/activate an AgencyProfile first.",
          empty: "No offers registered for this agency yet.",
          create: "Create offer",
          tourProductId: "TourProduct id",
          titleOverride: "Title override (optional)",
          back: "Back to dashboard",
          boundary: "Agency A cannot manage Agency B offers · Commission = deferred",
        };

  function create() {
    setError(null);
    startTransition(() => {
      void (async () => {
        const result = await createAgencyOfferAction({
          tourProductId: tourProductId.trim(),
          titleOverride: titleOverride.trim() || undefined,
        });
        if (!result.ok) {
          setError(result.message);
          return;
        }
        router.push(`/${locale}/agency/catalog/${result.item.id}`);
        router.refresh();
      })();
    });
  }

  return (
    <div className="flex flex-col gap-5">
      <header>
        <p className="text-xs font-semibold uppercase tracking-[0.14em] text-accent">
          {copy.eyebrow}
        </p>
        <h1 className="mt-2 text-2xl font-semibold tracking-tight text-foreground">
          {copy.title}
        </h1>
        <p className="mt-2 max-w-2xl text-sm text-muted-foreground">{copy.intro}</p>
        {profile ? (
          <p className="mt-2 text-sm text-muted-foreground">
            {profile.displayName} · <LtrValue>{profile.status}</LtrValue>
          </p>
        ) : null}
      </header>

      {loadError ? (
        <FieldMessage id="agency-offer-load-error" tone="error">
          {loadError}
        </FieldMessage>
      ) : null}

      {!profile ? (
        <Surface className="rounded-2xl p-6">
          <Text>{copy.profileMissing}</Text>
        </Surface>
      ) : (
        <>
          <Surface className="rounded-2xl p-6">
            <Text as="h2" role="heading" className="text-base font-semibold">
              {copy.create}
            </Text>
            <div className="mt-3 grid gap-3 sm:grid-cols-2">
              <label className="flex flex-col gap-1.5">
                <Text role="label">{copy.tourProductId}</Text>
                <LtrValue>
                  <input
                    className={`${fieldClass} w-full`}
                    value={tourProductId}
                    onChange={(e) => setTourProductId(e.target.value)}
                    placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                    required
                  />
                </LtrValue>
              </label>
              <label className="flex flex-col gap-1.5">
                <Text role="label">{copy.titleOverride}</Text>
                <input
                  className={fieldClass}
                  value={titleOverride}
                  onChange={(e) => setTitleOverride(e.target.value)}
                />
              </label>
            </div>
            {error ? (
              <FieldMessage id="agency-offer-create-error" tone="error" className="mt-3">
                {error}
              </FieldMessage>
            ) : null}
            <button
              type="button"
              className="mt-4 min-h-touch rounded-lg bg-accent px-4 text-sm font-semibold text-accent-foreground disabled:opacity-60"
              disabled={pending || !tourProductId.trim()}
              onClick={create}
            >
              {copy.create}
            </button>
          </Surface>

          <Surface className="rounded-2xl p-6">
            {items.length === 0 ? (
              <Text role="muted">{copy.empty}</Text>
            ) : (
              <ul className="flex flex-col gap-3">
                {items.map((item) => (
                  <li key={item.id}>
                    <Link
                      href={`/${locale}/agency/catalog/${item.id}`}
                      className="flex flex-col gap-1 rounded-xl border border-border p-3 hover:border-accent/40"
                    >
                      <span className="font-medium text-foreground">
                        {item.titleOverride || item.id}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        <LtrValue>
                          {item.publicationStatus} · {item.status} · {item.visibility} ·{" "}
                          {item.salesChannel}
                        </LtrValue>
                      </span>
                      <span className="text-xs text-muted-foreground">
                        TourProduct <LtrValue>{item.tourProductId}</LtrValue>
                      </span>
                    </Link>
                  </li>
                ))}
              </ul>
            )}
          </Surface>
        </>
      )}

      <Text role="caption" className="text-muted-foreground">
        {copy.boundary}
      </Text>
      <Link
        href={`/${locale}/agency`}
        className="min-h-touch inline-flex w-fit items-center rounded-lg border border-border px-4 text-sm font-medium hover:bg-surface"
      >
        {copy.back}
      </Link>
    </div>
  );
}

export function AgencyOfferOpsDetailView({
  locale,
  item,
}: {
  locale: AppLocale;
  item: AgencyOfferPanelItem;
}) {
  const router = useRouter();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);

  const actions: AgencyOfferLifecycleAction[] = [
    "activate",
    "list",
    "submit",
    "publish",
    "unpublish",
    "open-sales",
    "close-sales",
    "suspend",
    "retire",
  ];

  function run(action: AgencyOfferLifecycleAction) {
    setError(null);
    startTransition(() => {
      void (async () => {
        const result = await mutateAgencyOfferLifecycleAction(item.id, action);
        if (!result.ok) {
          setError(result.message);
          return;
        }
        router.refresh();
      })();
    });
  }

  return (
    <div className="flex flex-col gap-5">
      <header>
        <p className="text-xs font-semibold uppercase tracking-[0.14em] text-accent">
          Agency Offer detail
        </p>
        <h1 className="mt-2 text-2xl font-semibold tracking-tight">
          {item.titleOverride || item.id}
        </h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Lifecycle for the acting agency only · no commission/settlement UI
        </p>
      </header>

      <Surface className="rounded-2xl p-6">
        <dl className="grid gap-3 text-sm sm:grid-cols-2">
          <div>
            <dt className="text-muted-foreground">Publication</dt>
            <dd>
              <LtrValue>{item.publicationStatus}</LtrValue>
            </dd>
          </div>
          <div>
            <dt className="text-muted-foreground">Status / Visibility</dt>
            <dd>
              <LtrValue>
                {item.status} / {item.visibility}
              </LtrValue>
            </dd>
          </div>
          <div>
            <dt className="text-muted-foreground">Sales channel</dt>
            <dd>
              <LtrValue>{item.salesChannel}</LtrValue>
            </dd>
          </div>
          <div>
            <dt className="text-muted-foreground">Sales open</dt>
            <dd>
              <LtrValue>{String(item.salesOpen)}</LtrValue>
            </dd>
          </div>
          <div className="sm:col-span-2">
            <dt className="text-muted-foreground">TourProduct</dt>
            <dd>
              <LtrValue>{item.tourProductId}</LtrValue>
            </dd>
          </div>
          <div className="sm:col-span-2">
            <dt className="text-muted-foreground">Departure scope</dt>
            <dd>
              <LtrValue>
                {item.departureScopeMode}
                {item.departureScopeIds.length
                  ? ` · ${item.departureScopeIds.join(", ")}`
                  : ""}
              </LtrValue>
            </dd>
          </div>
        </dl>
      </Surface>

      <Surface className="rounded-2xl p-6">
        <Text as="h2" role="heading" className="text-base font-semibold">
          Lifecycle actions
        </Text>
        <div className="mt-3 flex flex-wrap gap-2">
          {actions.map((action) => (
            <button
              key={action}
              type="button"
              disabled={pending}
              className="min-h-touch rounded-lg border border-border px-3 text-sm font-medium hover:border-accent/40 disabled:opacity-60"
              onClick={() => run(action)}
            >
              {action}
            </button>
          ))}
        </div>
        {error ? (
          <FieldMessage id="agency-offer-lifecycle-error" tone="error" className="mt-3">
            {error}
          </FieldMessage>
        ) : null}
      </Surface>

      <Link
        href={`/${locale}/agency/catalog`}
        className="min-h-touch inline-flex w-fit items-center rounded-lg border border-border px-4 text-sm font-medium hover:bg-surface"
      >
        Back to offers
      </Link>
    </div>
  );
}
