"use client";

import { useState } from "react";
import type { AppLocale } from "@/lib/i18n";
import {
  AdminDataGrid,
  type AdminGridColumn,
  type AdminGridRow,
} from "@/features/admin-experience/admin-data-grid";

/**
 * Representative Operations board: shell consumers + grid + workflow patterns.
 * Pattern sample rows are explicitly non-operational.
 */
export function AdminOperationsBoard({ locale }: { locale: AppLocale }) {
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [step, setStep] = useState(0);
  const [showPatternRows, setShowPatternRows] = useState(true);
  const [page, setPage] = useState(1);
  const [sortColumnId, setSortColumnId] = useState<string | null>("code");
  const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");

  const copy =
    locale === "fa"
      ? {
          intro:
            "این سیستم قابل استفاده عملیاتی است — الگوهای Console برای اپراتور TravelCore.",
          filterBar: "نوار فیلتر",
          status: "وضعیت",
          all: "همه",
          draft: "پیش‌نویس",
          published: "منتشرشده",
          apply: "اعمال فیلتر",
          drawer: "کشو / جزئیات",
          openDrawer: "باز کردن کشو",
          closeDrawer: "بستن",
          stepper: "گردش مرحله‌ای",
          next: "مرحله بعد",
          back: "بازگشت",
          steps: ["اطلاعات پایه", "بازبینی", "انتشار"],
          gridTitle: "شبکه عملیاتی نمونه",
          gridLimit:
            "صفحه‌بندی/مرتب‌سازی/خروجی در سطح UI · اتصال کامل سرور ماژول‌به‌ماژول جداگانه است.",
          patternToggle: "نمایش ردیف‌های الگوی UI (نه داده زنده)",
          feedback: "بازخورد وضعیت",
          feedbackOk: "تغییرات ذخیره نشد — این فقط الگوی بازخورد است.",
        }
      : locale === "ar"
        ? {
            intro: "هذا النظام صالح للاستخدام التشغيلي — أنماط وحدة التحكم لمشغّل TravelCore.",
            filterBar: "شريط التصفية",
            status: "الحالة",
            all: "الكل",
            draft: "مسودة",
            published: "منشور",
            apply: "تطبيق التصفية",
            drawer: "درج / تفاصيل",
            openDrawer: "فتح الدرج",
            closeDrawer: "إغلاق",
            stepper: "سير مرحلي",
            next: "التالي",
            back: "رجوع",
            steps: ["معلومات أساسية", "مراجعة", "نشر"],
            gridTitle: "شبكة تشغيل نموذجية",
            gridLimit:
              "الترقيم/الترتيب/التصدير على مستوى الواجهة · ربط الخادم يتم لكل وحدة لاحقاً.",
            patternToggle: "إظهار صفوف نمط الواجهة (ليست بيانات حية)",
            feedback: "ملاحظات الحالة",
            feedbackOk: "لم تُحفظ تغييرات — هذا نمط ملاحظات فقط.",
          }
        : {
            intro:
              "This console is built for operational use — TravelCore operator patterns.",
            filterBar: "Filter bar",
            status: "Status",
            all: "All",
            draft: "Draft",
            published: "Published",
            apply: "Apply filters",
            drawer: "Drawer / detail",
            openDrawer: "Open drawer",
            closeDrawer: "Close",
            stepper: "Progressive workflow",
            next: "Next step",
            back: "Back",
            steps: ["Basics", "Review", "Publish"],
            gridTitle: "Sample operations grid",
            gridLimit:
              "Pagination/sort/export are UI contracts · per-module server wiring is separate.",
            patternToggle: "Show UI pattern rows (not live data)",
            feedback: "Status feedback",
            feedbackOk: "Nothing was saved — feedback pattern only.",
          };

  const columns: AdminGridColumn[] = [
    { id: "code", label: locale === "fa" ? "کد" : "Code", sortable: true, filterable: true },
    { id: "name", label: locale === "fa" ? "نام" : "Name", sortable: true, filterable: true },
    { id: "kind", label: locale === "fa" ? "نوع" : "Kind", sortable: true, filterable: true },
    {
      id: "status",
      label: locale === "fa" ? "وضعیت" : "Status",
      sortable: true,
      filterable: true,
    },
  ];

  const patternRows: AdminGridRow[] = showPatternRows
    ? [
        {
          id: "pattern-1",
          patternOnly: true,
          cells: {
            code: "PAT-001",
            name: locale === "fa" ? "نمونه ردیف عملیاتی" : "Sample ops row",
            kind: "Pattern",
            status: "Draft",
          },
        },
        {
          id: "pattern-2",
          patternOnly: true,
          cells: {
            code: "PAT-002",
            name: locale === "fa" ? "نمونه دوم" : "Second sample",
            kind: "Pattern",
            status: "Published",
          },
        },
        {
          id: "pattern-3",
          patternOnly: true,
          cells: {
            code: "PAT-003",
            name: locale === "fa" ? "نمونه سوم" : "Third sample",
            kind: "Pattern",
            status: "Draft",
          },
        },
      ]
    : [];

  const sorted = [...patternRows].sort((a, b) => {
    if (!sortColumnId) return 0;
    const av = a.cells[sortColumnId] ?? "";
    const bv = b.cells[sortColumnId] ?? "";
    const cmp = av.localeCompare(bv);
    return sortDir === "asc" ? cmp : -cmp;
  });

  return (
    <div className="flex flex-col gap-4 p-3 sm:p-4">
      <p className="text-sm text-muted-foreground">{copy.intro}</p>

      <section className="rounded-lg border border-border bg-surface p-3 shadow-sm">
        <h2 className="mb-2 text-sm font-semibold">{copy.filterBar}</h2>
        <form
          className="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-end"
          onSubmit={(e) => e.preventDefault()}
        >
          <label className="flex min-w-[10rem] flex-1 flex-col gap-1 text-xs">
            <span>{copy.status}</span>
            <select className="min-h-touch rounded-md border border-border bg-background px-2 text-sm">
              <option>{copy.all}</option>
              <option>{copy.draft}</option>
              <option>{copy.published}</option>
            </select>
          </label>
          <button
            type="submit"
            className="min-h-touch rounded-md bg-primary px-3 text-sm font-medium text-primary-foreground"
          >
            {copy.apply}
          </button>
          <button
            type="button"
            className="min-h-touch rounded-md border border-border px-3 text-sm"
            onClick={() => setDrawerOpen(true)}
          >
            {copy.openDrawer}
          </button>
        </form>
      </section>

      <label className="inline-flex items-center gap-2 text-xs text-muted-foreground">
        <input
          type="checkbox"
          checked={showPatternRows}
          onChange={(e) => {
            setShowPatternRows(e.target.checked);
            setPage(1);
          }}
        />
        {copy.patternToggle}
      </label>

      <AdminDataGrid
        locale={locale}
        title={copy.gridTitle}
        limitationsNote={copy.gridLimit}
        columns={columns}
        rows={sorted}
        page={page}
        pageSize={5}
        onPageChange={setPage}
        sortColumnId={sortColumnId}
        sortDir={sortDir}
        onSortChange={(id) => {
          if (sortColumnId === id) {
            setSortDir((d) => (d === "asc" ? "desc" : "asc"));
          } else {
            setSortColumnId(id);
            setSortDir("asc");
          }
        }}
      />

      <section className="rounded-lg border border-border bg-surface p-3 shadow-sm">
        <h2 className="mb-2 text-sm font-semibold">{copy.stepper}</h2>
        <ol className="mb-3 flex flex-wrap gap-2 text-xs">
          {copy.steps.map((label, i) => (
            <li
              key={label}
              className={
                i === step
                  ? "rounded-full bg-primary px-3 py-1 font-medium text-primary-foreground"
                  : "rounded-full border border-border px-3 py-1"
              }
            >
              {i + 1}. {label}
            </li>
          ))}
        </ol>
        <div className="flex gap-2">
          <button
            type="button"
            className="min-h-touch rounded-md border border-border px-3 text-sm disabled:opacity-40"
            disabled={step === 0}
            onClick={() => setStep((s) => Math.max(0, s - 1))}
          >
            {copy.back}
          </button>
          <button
            type="button"
            className="min-h-touch rounded-md bg-primary px-3 text-sm text-primary-foreground disabled:opacity-40"
            disabled={step >= copy.steps.length - 1}
            onClick={() => setStep((s) => Math.min(copy.steps.length - 1, s + 1))}
          >
            {copy.next}
          </button>
        </div>
      </section>

      <section
        role="status"
        className="rounded-md border border-border bg-surface-muted/60 px-3 py-2 text-xs text-muted-foreground"
      >
        <strong className="text-foreground">{copy.feedback}: </strong>
        {copy.feedbackOk}
      </section>

      {drawerOpen ? (
        <div
          role="dialog"
          aria-modal="true"
          className="fixed inset-0 z-50 flex justify-end bg-foreground/40"
        >
          <div className="flex h-full w-full max-w-md flex-col border-s border-border bg-surface shadow-xl">
            <div className="flex items-center justify-between border-b border-border px-3 py-2">
              <h3 className="text-sm font-semibold">{copy.drawer}</h3>
              <button
                type="button"
                className="min-h-touch rounded-md border border-border px-2 text-xs"
                onClick={() => setDrawerOpen(false)}
              >
                {copy.closeDrawer}
              </button>
            </div>
            <div className="flex-1 overflow-auto p-3 text-sm text-muted-foreground">
              {locale === "fa"
                ? "کشو برای جزئیات ردیف / فرم کوتاه · الگوی تعامل ادمین."
                : "Drawer for row detail / short form · admin interaction pattern."}
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}
