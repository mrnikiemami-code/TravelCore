import Link from "next/link";
import { Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

/**
 * Admin Console operations dashboard foundation (TC-P37-T004).
 * Workflow-oriented ops surface · honest empties · no fake KPIs/revenue.
 */
export function AdminConsoleFoundation({ locale }: { locale: AppLocale }) {
  const base = `/${locale}/admin`;
  const copy =
    locale === "fa"
      ? {
          feeling: "این کنسول عملیاتی است — نه داشبورد مسافر و نه پورتال فروش آژانس.",
          intro:
            "عملیات کاتالوگ، محتوا، آژانس، دسترسی و جهت ممیزی/گزارش — بدون عدد جعلی.",
          catalog: "عملیات کاتالوگ",
          catalogBody: "گردش‌کار دامنه: هتل و تور به‌صورت گام‌به‌گام — نه منوهای CRUD جدا.",
          openCatalog: "باز کردن عملیات کاتالوگ",
          content: "محتوا و رسانه",
          contentBody: "عملیات رسانه و محتوا — بدون شمارنده جعلی.",
          agencies: "مدیریت آژانس",
          agenciesBody: "رابطه تجاری آژانس → کاربران → دسترسی — بدون آژانس جعلی.",
          access: "کاربر و دسترسی",
          accessBody: "Identity ≠ Party ≠ Access — نقش‌ها هاردکد نمی‌شوند.",
          reporting: "گزارش‌ها",
          reportingBody: "جهت گزارش آینده — بدون درآمد/فروش جعلی.",
          audit: "ممیزی و گردش‌کار",
          auditBody: "جهت ممیزی عملیاتی — بدون لاگ جعلی.",
          honest: "خالی · صادقانه",
          open: "باز کردن",
          legacyOps: "برد الگوهای داده (موجود)",
        }
      : locale === "ar"
        ? {
            feeling: "هذه وحدة تشغيلية — وليست لوحة المسافر ولا بوابة مبيعات الوكالة.",
            intro:
              "عمليات الكتالوج والمحتوى والوكالات والوصول واتجاه التدقيق/التقارير — دون أرقام وهمية.",
            catalog: "عمليات الكتالوج",
            catalogBody: "سير عمل المجال: فندق وجولة خطوة بخطوة — لا قوائم CRUD منفصلة.",
            openCatalog: "فتح عمليات الكتالوج",
            content: "المحتوى والوسائط",
            contentBody: "عمليات الوسائط والمحتوى — بلا عدّادات وهمية.",
            agencies: "إدارة الوكالات",
            agenciesBody: "علاقة تجارية: وكالة → مستخدمون → وصول — بلا وكالات وهمية.",
            access: "المستخدم والوصول",
            accessBody: "Identity ≠ Party ≠ Access — بلا أدوار ثابتة.",
            reporting: "التقارير",
            reportingBody: "اتجاه تقارير مستقبلي — بلا إيرادات/مبيعات وهمية.",
            audit: "التدقيق وسير العمل",
            auditBody: "اتجاه تدقيق تشغيلي — بلا سجلات وهمية.",
            honest: "فارغ · بصدق",
            open: "فتح",
            legacyOps: "لوحة أنماط البيانات (موجودة)",
          }
        : {
            feeling:
              "This is an operational console — not the traveler dashboard or agency sales portal.",
            intro:
              "Catalog ops, content, agencies, access, and audit/reporting direction — no invented numbers.",
            catalog: "Catalog operations",
            catalogBody:
              "Domain workflows: Hotel and Tour as step sequences — not unrelated CRUD menus.",
            openCatalog: "Open catalog operations",
            content: "Content & media",
            contentBody: "Media and content operations — no fake counters.",
            agencies: "Agency management",
            agenciesBody:
              "Commercial relationship: Agency → Users → Access — no invented agencies.",
            access: "Users & access",
            accessBody: "Identity ≠ Party ≠ Access — roles are not hardcoded.",
            reporting: "Reporting",
            reportingBody: "Future reporting direction — no fake revenue or sales.",
            audit: "Audit & workflow",
            auditBody: "Operational audit direction — no invented logs.",
            honest: "Empty · honest",
            open: "Open",
            legacyOps: "Data-pattern board (existing)",
          };

  const cards = [
    {
      title: copy.catalog,
      body: copy.catalogBody,
      href: `${base}/catalog-ops`,
      cta: copy.openCatalog,
    },
    {
      title: copy.content,
      body: copy.contentBody,
      href: `${base}/content`,
      cta: copy.open,
    },
    {
      title: copy.agencies,
      body: copy.agenciesBody,
      href: `${base}/agencies`,
      cta: copy.open,
    },
    {
      title: copy.access,
      body: copy.accessBody,
      href: `${base}/access`,
      cta: copy.open,
    },
    {
      title: copy.reporting,
      body: copy.reportingBody,
      href: `${base}/reporting`,
      cta: copy.open,
    },
    {
      title: copy.audit,
      body: copy.auditBody,
      href: `${base}/audit`,
      cta: copy.open,
    },
  ];

  return (
    <div className="flex flex-col gap-5">
      <Surface className="rounded-2xl border-primary/30 bg-gradient-to-br from-primary/10 via-surface to-surface p-5 sm:p-6">
        <p className="text-xs font-semibold uppercase tracking-[0.14em] text-primary">
          OPS
        </p>
        <h1 className="mt-2 text-xl font-semibold tracking-tight text-foreground sm:text-2xl">
          {copy.feeling}
        </h1>
        <p className="mt-2 max-w-2xl text-sm text-muted-foreground">{copy.intro}</p>
        <p className="mt-3 text-[11px] text-muted-foreground">
          Admin ≠ Customer · Admin ≠ Agency · Admin ≠ CRUD generator
        </p>
      </Surface>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
        {cards.map((card) => (
          <Surface key={card.href} className="flex flex-col rounded-xl p-4 sm:p-5">
            <span className="w-fit rounded-full bg-surface-muted px-2 py-0.5 text-[10px] text-muted-foreground">
              {copy.honest}
            </span>
            <Text as="h2" role="heading" className="mt-2 text-base font-semibold">
              {card.title}
            </Text>
            <Text role="muted" className="mt-1.5 flex-1 text-sm">
              {card.body}
            </Text>
            <Link
              href={card.href}
              className="mt-4 inline-flex min-h-touch items-center text-sm font-semibold text-primary hover:underline"
            >
              {card.cta}
            </Link>
          </Surface>
        ))}
      </div>

      <Surface className="rounded-xl border-dashed p-4">
        <Text role="muted" className="text-sm">
          {copy.legacyOps}
        </Text>
        <Link
          href={`${base}/operations`}
          className="mt-2 inline-flex min-h-touch items-center text-sm font-medium text-primary hover:underline"
        >
          /admin/operations
        </Link>
      </Surface>
    </div>
  );
}
