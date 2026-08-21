import Link from "next/link";
import { Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

export type AgencySectionId =
  | "catalog"
  | "bookings"
  | "customers"
  | "commission"
  | "settlement"
  | "users"
  | "profile";

export function AgencySectionView({
  locale,
  section,
}: {
  locale: AppLocale;
  section: AgencySectionId;
}) {
  const c = sectionCopy(locale, section);
  return (
    <div className="flex flex-col gap-5">
      <header>
        <p className="text-xs font-semibold uppercase tracking-[0.14em] text-accent">
          {c.eyebrow}
        </p>
        <h1 className="mt-2 text-2xl font-semibold tracking-tight text-foreground">
          {c.title}
        </h1>
        <p className="mt-2 max-w-2xl text-sm text-muted-foreground">{c.intro}</p>
      </header>
      <Surface className="rounded-2xl p-6 sm:p-8">
        <span className="rounded-full bg-surface-muted px-2.5 py-1 text-[11px] text-muted-foreground">
          {c.badge}
        </span>
        <Text as="h2" role="heading" className="mt-3 text-lg font-semibold">
          {c.emptyTitle}
        </Text>
        <Text role="muted" className="mt-2">
          {c.emptyBody}
        </Text>
        {c.boundary ? (
          <Text role="caption" className="mt-3 text-muted-foreground">
            {c.boundary}
          </Text>
        ) : null}
        <div className="mt-4 flex flex-wrap gap-2">
          {c.primaryHref ? (
            <Link
              href={c.primaryHref}
              className="min-h-touch inline-flex items-center rounded-lg bg-accent px-4 text-sm font-semibold text-accent-foreground hover:opacity-95"
            >
              {c.primaryCta}
            </Link>
          ) : null}
          <Link
            href={`/${locale}/agency`}
            className="min-h-touch inline-flex items-center rounded-lg border border-border px-4 text-sm font-medium hover:bg-surface"
          >
            {c.back}
          </Link>
        </div>
      </Surface>
    </div>
  );
}

function sectionCopy(locale: AppLocale, section: AgencySectionId) {
  const en = {
    eyebrow: "Agency B2B",
    badge: "Empty · honest",
    back: "Back to dashboard",
    catalog: {
      title: "Sellable catalog",
      intro: "Agency selling view over published catalog facts — not consumer cards.",
      emptyTitle: "Agency catalog list not wired yet",
      emptyBody:
        "Use the public tour/hotel surfaces for published facts today. Agency-specific pricing/commission display waits on commercial contracts — we do not invent margins.",
      boundary: "Agency Experience ≠ Public + Role Toggle",
      primaryCta: "Browse public tours (facts)",
      primaryHref: `/${locale}/tours`,
    },
    bookings: {
      title: "Agency bookings",
      intro: "Agency-scoped booking operations — not traveler My Trips.",
      emptyTitle: "No agency bookings to show",
      emptyBody: "When agency access contracts exist, operational booking rows appear here.",
      boundary: "Booking ≠ Payment · FE ≠ Source of Truth",
      primaryCta: "Dashboard",
      primaryHref: `/${locale}/agency`,
    },
    customers: {
      title: "Agency customers",
      intro: "Customers belonging to the agency sales relationship.",
      emptyTitle: "No customer list yet",
      emptyBody: "Live agency customer roster is not connected on this foundation layer.",
      boundary: null,
      primaryCta: "Dashboard",
      primaryHref: `/${locale}/agency`,
    },
    commission: {
      title: "Commission direction",
      intro: "Commercial terms visibility — never invented percentages.",
      emptyTitle: "No commission facts available",
      emptyBody: "Commission amounts appear only from authorized commercial profile/offer terms.",
      boundary: "No fake commission / margin",
      primaryCta: "Commercial profile",
      primaryHref: `/${locale}/agency/profile`,
    },
    settlement: {
      title: "Settlement direction",
      intro: "Future settlement UX for agency money movement.",
      emptyTitle: "Settlement not activated",
      emptyBody: "No fake balances, payouts, or statements are shown.",
      boundary: "No fake settlement",
      primaryCta: "Dashboard",
      primaryHref: `/${locale}/agency`,
    },
    users: {
      title: "Agency users",
      intro: "Team members via Access — owner/staff ready, not hardcoded roles.",
      emptyTitle: "No team roster wired",
      emptyBody: "AgencyMember / access relationships will populate this when authorized.",
      boundary: "Identity ≠ Party ≠ Access",
      primaryCta: "Dashboard",
      primaryHref: `/${locale}/agency`,
    },
    profile: {
      title: "Commercial profile",
      intro: "Agency commercial profile and capability references.",
      emptyTitle: "Profile foundation only",
      emptyBody: "Commercial profile facts appear when Marketplace agency contracts are available.",
      boundary: "Agency ≠ Customer ≠ Admin",
      primaryCta: "Dashboard",
      primaryHref: `/${locale}/agency`,
    },
  };

  const pack = en; // fa/ar can share EN structure for foundation; keep concise
  if (locale === "fa") {
    return {
      eyebrow: "آژانس B2B",
      badge: "خالی · صادقانه",
      back: "بازگشت به داشبورد",
      title: sectionTitleFa(section),
      intro: pack[section].intro,
      emptyTitle: pack[section].emptyTitle,
      emptyBody: pack[section].emptyBody,
      boundary: pack[section].boundary,
      primaryCta: pack[section].primaryCta,
      primaryHref: pack[section].primaryHref,
    };
  }
  if (locale === "ar") {
    return {
      eyebrow: "وكالة B2B",
      badge: "فارغ · بصدق",
      back: "العودة إلى اللوحة",
      title: sectionTitleAr(section),
      intro: pack[section].intro,
      emptyTitle: pack[section].emptyTitle,
      emptyBody: pack[section].emptyBody,
      boundary: pack[section].boundary,
      primaryCta: pack[section].primaryCta,
      primaryHref: pack[section].primaryHref,
    };
  }
  return {
    eyebrow: pack.eyebrow,
    badge: pack.badge,
    back: pack.back,
    title: pack[section].title,
    intro: pack[section].intro,
    emptyTitle: pack[section].emptyTitle,
    emptyBody: pack[section].emptyBody,
    boundary: pack[section].boundary,
    primaryCta: pack[section].primaryCta,
    primaryHref: pack[section].primaryHref,
  };
}

function sectionTitleFa(section: AgencySectionId): string {
  switch (section) {
    case "catalog":
      return "کاتالوگ فروش";
    case "bookings":
      return "رزروهای آژانس";
    case "customers":
      return "مشتریان";
    case "commission":
      return "کمیسیون";
    case "settlement":
      return "تسویه";
    case "users":
      return "کاربران آژانس";
    case "profile":
      return "پروفایل تجاری";
  }
}

function sectionTitleAr(section: AgencySectionId): string {
  switch (section) {
    case "catalog":
      return "كتالوج البيع";
    case "bookings":
      return "حجوزات الوكالة";
    case "customers":
      return "العملاء";
    case "commission":
      return "العمولة";
    case "settlement":
      return "التسوية";
    case "users":
      return "مستخدمو الوكالة";
    case "profile":
      return "الملف التجاري";
  }
}
