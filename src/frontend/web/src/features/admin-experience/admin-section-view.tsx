import Link from "next/link";
import { Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

export type AdminSectionId =
  | "catalog-ops"
  | "content"
  | "agencies"
  | "access"
  | "reporting"
  | "audit"
  | "profile";

export function AdminSectionView({
  locale,
  section,
}: {
  locale: AppLocale;
  section: AdminSectionId;
}) {
  const c = sectionCopy(locale, section);
  return (
    <div className="flex flex-col gap-5">
      <header>
        <p className="text-xs font-semibold uppercase tracking-[0.14em] text-primary">
          {c.eyebrow}
        </p>
        <h1 className="mt-2 text-2xl font-semibold tracking-tight text-foreground">
          {c.title}
        </h1>
        <p className="mt-2 max-w-2xl text-sm text-muted-foreground">{c.intro}</p>
      </header>

      {c.workflows && c.workflows.length > 0 ? (
        <div className="grid gap-3 sm:grid-cols-2">
          {c.workflows.map((wf) => (
            <Surface key={wf.title} className="rounded-xl p-4 sm:p-5">
              <Text as="h2" role="heading" className="text-base font-semibold">
                {wf.title}
              </Text>
              <ol className="mt-3 list-decimal space-y-1 ps-5 text-sm text-muted-foreground">
                {wf.steps.map((step) => (
                  <li key={step}>{step}</li>
                ))}
              </ol>
              {wf.href ? (
                <Link
                  href={wf.href}
                  className="mt-4 inline-flex min-h-touch items-center text-sm font-semibold text-primary hover:underline"
                >
                  {wf.cta}
                </Link>
              ) : null}
            </Surface>
          ))}
        </div>
      ) : null}

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
              className="min-h-touch inline-flex items-center rounded-lg bg-primary px-4 text-sm font-semibold text-primary-foreground hover:opacity-95"
            >
              {c.primaryCta}
            </Link>
          ) : null}
          <Link
            href={`/${locale}/admin`}
            className="min-h-touch inline-flex items-center rounded-lg border border-border px-4 text-sm font-medium hover:bg-surface"
          >
            {c.back}
          </Link>
        </div>
      </Surface>
    </div>
  );
}

function sectionCopy(locale: AppLocale, section: AdminSectionId) {
  const en = {
    eyebrow: "Admin Ops",
    badge: "Empty · honest",
    back: "Back to dashboard",
    "catalog-ops": {
      title: "Catalog operations",
      intro:
        "Domain-oriented publish workflows — not Hotel / Room / Facility as unrelated CRUD menus.",
      emptyTitle: "Live ops queues are not inventing counts",
      emptyBody:
        "Use the workflow cards above to enter existing catalog islands. We do not show fake pending/publish totals.",
      boundary: "Admin Console ≠ CRUD generator · FE ≠ Source of Truth",
      primaryCta: "Open tour catalog island",
      primaryHref: `/${locale}/admin/catalog/tours`,
      workflows: [
        {
          title: "Hotel management",
          steps: ["Basic info", "Rooms", "Media", "Facilities", "Policies", "Publish"],
          href: `/${locale}/admin/catalog/places`,
          cta: "Open places workflow",
        },
        {
          title: "Tour management",
          steps: [
            "Tour product",
            "Destinations",
            "Departures",
            "Pricing references",
            "Content",
            "Publish",
          ],
          href: `/${locale}/admin/catalog/tours`,
          cta: "Open tours workflow",
        },
      ],
    },
    content: {
      title: "Content & media operations",
      intro: "Operational media and content tooling — dense, not marketing layout.",
      emptyTitle: "No invented media queue size",
      emptyBody: "Open the existing media surface when assets need work. Counts stay honest.",
      boundary: null,
      primaryCta: "Open media",
      primaryHref: `/${locale}/admin/media`,
      workflows: [],
    },
    agencies: {
      title: "Agency management",
      intro: "Agency → Users → Access → commercial relationship — not a raw party table dump.",
      emptyTitle: "No fake agency roster",
      emptyBody:
        "Agency management foundation is ready as direction. Live agency rows appear when Access/Party contracts are wired — we do not invent agencies.",
      boundary: "Agency Portal ≠ Admin Console",
      primaryCta: "Offer governance queue",
      primaryHref: `/${locale}/admin/agencies/offers`,
      workflows: [
        {
          title: "Agency offer governance",
          steps: ["Agency submits", "Admin reviews", "Approve / Reject", "Published / Suspend"],
          href: `/${locale}/admin/agencies/offers`,
          cta: "Open offer queue",
        },
        {
          title: "Agency relationship",
          steps: ["Agency", "Users", "Access", "Commercial relationship"],
          href: `/${locale}/admin/accounts`,
          cta: "Accounts direction",
        },
      ],
    },
    access: {
      title: "Users & access",
      intro: "Permission-aware foundation — Identity ≠ Party ≠ Access.",
      emptyTitle: "No hardcoded fake roles",
      emptyBody:
        "Access awareness is prepared in navigation and copy. Live permission matrices are not fabricated here.",
      boundary: "Customer ≠ Agency User ≠ Admin User",
      primaryCta: "Open accounts",
      primaryHref: `/${locale}/admin/accounts`,
      workflows: [],
    },
    reporting: {
      title: "Reporting direction",
      intro: "Future operational reporting — not a fake analytics wall.",
      emptyTitle: "No fake revenue or booking charts",
      emptyBody: "Reporting remains a direction card until real aggregates exist.",
      boundary: "Booking ≠ Payment",
      primaryCta: "Dashboard",
      primaryHref: `/${locale}/admin`,
      workflows: [],
    },
    audit: {
      title: "Audit & workflow",
      intro: "Operational audit and workflow direction for publish/moderation paths.",
      emptyTitle: "No invented audit trail",
      emptyBody: "UGC moderation island remains available; full audit ledger waits on contracts.",
      boundary: null,
      primaryCta: "UGC moderation",
      primaryHref: `/${locale}/admin/ugc/moderation`,
      workflows: [],
    },
    profile: {
      title: "Operator profile",
      intro: "Admin operator context — separate from traveler profile and agency commercial profile.",
      emptyTitle: "Profile binding not mocked",
      emptyBody: "Operator identity comes from Access when available — no fake admin persona.",
      boundary: "Identity ≠ Party ≠ Access",
      primaryCta: "Dashboard",
      primaryHref: `/${locale}/admin`,
      workflows: [],
    },
  };

  if (locale === "fa") {
    return {
      eyebrow: "عملیات ادمین",
      badge: "خالی · صادقانه",
      back: "بازگشت به داشبورد",
      title: faTitle(section),
      intro: en[section].intro,
      emptyTitle: en[section].emptyTitle,
      emptyBody: en[section].emptyBody,
      boundary: en[section].boundary,
      primaryCta: en[section].primaryCta,
      primaryHref: en[section].primaryHref,
      workflows: en[section].workflows,
    };
  }
  if (locale === "ar") {
    return {
      eyebrow: "تشغيل الإدارة",
      badge: "فارغ · بصدق",
      back: "العودة إلى اللوحة",
      title: arTitle(section),
      intro: en[section].intro,
      emptyTitle: en[section].emptyTitle,
      emptyBody: en[section].emptyBody,
      boundary: en[section].boundary,
      primaryCta: en[section].primaryCta,
      primaryHref: en[section].primaryHref,
      workflows: en[section].workflows,
    };
  }
  return {
    eyebrow: en.eyebrow,
    badge: en.badge,
    back: en.back,
    title: en[section].title,
    intro: en[section].intro,
    emptyTitle: en[section].emptyTitle,
    emptyBody: en[section].emptyBody,
    boundary: en[section].boundary,
    primaryCta: en[section].primaryCta,
    primaryHref: en[section].primaryHref,
    workflows: en[section].workflows,
  };
}

function faTitle(section: AdminSectionId): string {
  const map: Record<AdminSectionId, string> = {
    "catalog-ops": "عملیات کاتالوگ",
    content: "محتوا و رسانه",
    agencies: "مدیریت آژانس",
    access: "کاربر و دسترسی",
    reporting: "گزارش‌ها",
    audit: "ممیزی و گردش‌کار",
    profile: "پروفایل اپراتور",
  };
  return map[section];
}

function arTitle(section: AdminSectionId): string {
  const map: Record<AdminSectionId, string> = {
    "catalog-ops": "عمليات الكتالوج",
    content: "المحتوى والوسائط",
    agencies: "إدارة الوكالات",
    access: "المستخدم والوصول",
    reporting: "التقارير",
    audit: "التدقيق وسير العمل",
    profile: "ملف المشغّل",
  };
  return map[section];
}
