import Link from "next/link";
import { Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

/**
 * Agency Portal dashboard foundation (TC-P37-T003).
 * B2B sales tool · honest empties · no fake commissions/revenue/KPIs.
 */
export function AgencyDashboardFoundation({ locale }: { locale: AppLocale }) {
  const base = `/${locale}/agency`;
  const copy =
    locale === "fa"
      ? {
          feeling: "این ابزار فروش B2B است — نه سایت عمومی با نقش اضافه.",
          intro:
            "کاتالوگ قابل فروش، رزرو آژانس، مشتری و جهت کمیسیون/تسویه — بدون عدد جعلی.",
          catalog: "کاتالوگ فروش",
          catalogBody: "تورهای قابل فروش برای آژانس — نه همان کارت‌های مصرف‌کننده.",
          openCatalog: "باز کردن کاتالوگ",
          bookings: "رزروهای آژانس",
          bookingsBody: "وقتی قرارداد دسترسی آماده باشد، رزروهای آژانس اینجا می‌آید.",
          customers: "مشتریان آژانس",
          customersBody: "فهرست مشتری زنده در این لایه هنوز متصل نیست.",
          commission: "کمیسیون",
          commissionBody: "درصد/مبلغ کمیسیون جعلی نشان داده نمی‌شود.",
          settlement: "تسویه",
          settlementBody: "جهت تسویه آینده — بدون موجودی جعلی.",
          users: "کاربران و دسترسی",
          usersBody: "مالک/کارمند آژانس از Access می‌آید — نقش هاردکد نمی‌شود.",
          honest: "خالی · صادقانه",
          open: "باز کردن",
        }
      : locale === "ar"
        ? {
            feeling: "هذه أداة مبيعات B2B — وليست موقعاً عاماً بدور إضافي.",
            intro:
              "كتالوج قابل للبيع وحجوزات الوكالة والعملاء واتجاه العمولة/التسوية — دون أرقام وهمية.",
            catalog: "كتالوج البيع",
            catalogBody: "جولات قابلة للبيع للوكالة — وليست بطاقات المستهلك.",
            openCatalog: "فتح الكتالوج",
            bookings: "حجوزات الوكالة",
            bookingsBody: "ستظهر حجوزات الوكالة عند جاهزية عقد الوصول.",
            customers: "عملاء الوكالة",
            customersBody: "لا قائمة عملاء حية في هذه الطبقة بعد.",
            commission: "العمولة",
            commissionBody: "لا نعرض نسب/مبالغ عمولة وهمية.",
            settlement: "التسوية",
            settlementBody: "اتجاه تسوية مستقبلي — بلا أرصدة وهمية.",
            users: "المستخدمون والصلاحيات",
            usersBody: "مالك/موظف الوكالة من Access — بلا أدوار ثابتة.",
            honest: "فارغ · بصدق",
            open: "فتح",
          }
        : {
            feeling: "This is a B2B sales tool — not the public site with an extra role.",
            intro:
              "Sellable catalog, agency bookings, customers, and commission/settlement direction — no invented numbers.",
            catalog: "Sellable catalog",
            catalogBody: "Agency sell path for tours — not consumer marketplace cards.",
            openCatalog: "Open catalog",
            bookings: "Agency bookings",
            bookingsBody: "Agency bookings appear when access contracts are ready.",
            customers: "Agency customers",
            customersBody: "No live customer list on this layer yet.",
            commission: "Commission",
            commissionBody: "We do not invent commission rates or amounts.",
            settlement: "Settlement",
            settlementBody: "Future settlement direction — no fake balances.",
            users: "Users & access",
            usersBody: "Agency owner/staff come from Access — roles are not hardcoded.",
            honest: "Empty · honest",
            open: "Open",
          };

  const cards = [
    {
      title: copy.catalog,
      body: copy.catalogBody,
      href: `${base}/catalog`,
      cta: copy.openCatalog,
    },
    {
      title: copy.bookings,
      body: copy.bookingsBody,
      href: `${base}/bookings`,
      cta: copy.open,
    },
    {
      title: copy.customers,
      body: copy.customersBody,
      href: `${base}/customers`,
      cta: copy.open,
    },
    {
      title: copy.commission,
      body: copy.commissionBody,
      href: `${base}/commission`,
      cta: copy.open,
    },
    {
      title: copy.settlement,
      body: copy.settlementBody,
      href: `${base}/settlement`,
      cta: copy.open,
    },
    {
      title: copy.users,
      body: copy.usersBody,
      href: `${base}/users`,
      cta: copy.open,
    },
  ];

  return (
    <div className="flex flex-col gap-5">
      <Surface className="rounded-2xl border-accent/30 bg-gradient-to-br from-accent/15 via-surface to-surface p-5 sm:p-6">
        <p className="text-xs font-semibold uppercase tracking-[0.14em] text-accent">
          B2B
        </p>
        <h1 className="mt-2 text-xl font-semibold tracking-tight text-foreground sm:text-2xl">
          {copy.feeling}
        </h1>
        <Text role="muted" className="mt-2 max-w-3xl text-sm">
          {copy.intro}
        </Text>
      </Surface>

      <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {cards.map((card) => (
          <li key={card.href}>
            <Surface className="flex h-full flex-col rounded-2xl p-5">
              <div className="flex items-start justify-between gap-2">
                <h2 className="text-base font-semibold text-foreground">
                  {card.title}
                </h2>
                <span className="rounded-full bg-surface-muted px-2 py-0.5 text-[10px] text-muted-foreground">
                  {copy.honest}
                </span>
              </div>
              <Text role="muted" className="mt-2 flex-1 text-sm">
                {card.body}
              </Text>
              <Link
                href={card.href}
                className="mt-4 inline-flex min-h-touch items-center justify-center rounded-lg bg-accent px-3 text-sm font-semibold text-accent-foreground hover:opacity-95"
              >
                {card.cta}
              </Link>
            </Surface>
          </li>
        ))}
      </ul>
    </div>
  );
}
