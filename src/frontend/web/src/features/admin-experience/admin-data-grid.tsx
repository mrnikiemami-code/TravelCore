"use client";

import { useMemo, useState } from "react";
import type { AppLocale } from "@/lib/i18n";

export type AdminGridColumn = {
  id: string;
  label: string;
  sortable?: boolean;
  filterable?: boolean;
};

export type AdminGridRow = {
  id: string;
  cells: Record<string, string>;
  /** When true, row is UI-pattern only — not live operational data. */
  patternOnly?: boolean;
};

export type AdminDataGridProps = {
  locale: AppLocale;
  columns: AdminGridColumn[];
  rows: AdminGridRow[];
  loading?: boolean;
  error?: string | null;
  page?: number;
  pageSize?: number;
  totalCount?: number;
  onPageChange?: (page: number) => void;
  /** Server-side sort contract surface (UI only until API wired). */
  sortColumnId?: string | null;
  sortDir?: "asc" | "desc";
  onSortChange?: (columnId: string) => void;
  title?: string;
  limitationsNote?: string;
};

/**
 * Reusable Admin data-grid experience pattern (TC-P30-T008).
 * UI/interaction foundation — does not invent backend capabilities.
 */
export function AdminDataGrid({
  locale,
  columns,
  rows,
  loading = false,
  error = null,
  page = 1,
  pageSize = 10,
  totalCount,
  onPageChange,
  sortColumnId = null,
  sortDir = "asc",
  onSortChange,
  title,
  limitationsNote,
}: AdminDataGridProps) {
  const [visible, setVisible] = useState<Record<string, boolean>>(() =>
    Object.fromEntries(columns.map((c) => [c.id, true])),
  );
  const [selected, setSelected] = useState<Record<string, boolean>>({});
  const [columnFilters, setColumnFilters] = useState<Record<string, string>>({});
  const [query, setQuery] = useState("");
  const [showColumns, setShowColumns] = useState(false);
  const [confirmBulk, setConfirmBulk] = useState(false);

  const copy =
    locale === "fa"
      ? {
          search: "جستجو",
          columns: "ستون‌ها",
          export: "خروجی",
          exportNote: "خروجی به API متصل نیست · فقط سطح UI",
          bulk: "اقدام گروهی",
          selectAll: "انتخاب همه",
          empty: "ردیفی برای نمایش نیست",
          loading: "در حال بارگذاری…",
          error: "بارگذاری ناموفق بود",
          prev: "قبلی",
          next: "بعدی",
          page: "صفحه",
          selected: "انتخاب‌شده",
          actions: "اقدامات",
          open: "باز کردن",
          savedViews: "نمای ذخیره‌شده",
          savedViewsNote: "الگوی UI · ذخیره سمت سرور هنوز متصل نیست",
          patternBadge: "الگوی UI · نه داده عملیاتی زنده",
          confirmTitle: "تأیید اقدام گروهی",
          confirmBody: "این اقدام نمایشی است و تغییری در داده اعمال نمی‌کند.",
          confirmOk: "متوجه شدم",
          cancel: "انصراف",
          filter: "فیلتر",
        }
      : locale === "ar"
        ? {
            search: "بحث",
            columns: "الأعمدة",
            export: "تصدير",
            exportNote: "التصدير غير متصل بـ API · واجهة فقط",
            bulk: "إجراء جماعي",
            selectAll: "تحديد الكل",
            empty: "لا صفوف للعرض",
            loading: "جارٍ التحميل…",
            error: "فشل التحميل",
            prev: "السابق",
            next: "التالي",
            page: "صفحة",
            selected: "محدد",
            actions: "إجراءات",
            open: "فتح",
            savedViews: "عرض محفوظ",
            savedViewsNote: "نمط واجهة · الحفظ على الخادم غير متصل",
            patternBadge: "نمط واجهة · ليس بيانات تشغيل حية",
            confirmTitle: "تأكيد الإجراء الجماعي",
            confirmBody: "هذا إجراء عرضي ولا يغيّر البيانات.",
            confirmOk: "حسناً",
            cancel: "إلغاء",
            filter: "تصفية",
          }
        : {
            search: "Search",
            columns: "Columns",
            export: "Export",
            exportNote: "Export is not API-backed · UI surface only",
            bulk: "Bulk action",
            selectAll: "Select all",
            empty: "No rows to show",
            loading: "Loading…",
            error: "Failed to load",
            prev: "Previous",
            next: "Next",
            page: "Page",
            selected: "selected",
            actions: "Actions",
            open: "Open",
            savedViews: "Saved view",
            savedViewsNote: "UI affordance · server save not wired",
            patternBadge: "UI pattern · not live ops data",
            confirmTitle: "Confirm bulk action",
            confirmBody: "This is a presentation action and does not mutate data.",
            confirmOk: "Got it",
            cancel: "Cancel",
            filter: "Filter",
          };

  const visibleColumns = columns.filter((c) => visible[c.id] !== false);

  const filteredRows = useMemo(() => {
    let list = rows;
    const q = query.trim().toLowerCase();
    if (q) {
      list = list.filter((r) =>
        Object.values(r.cells).some((v) => v.toLowerCase().includes(q)),
      );
    }
    for (const [colId, fv] of Object.entries(columnFilters)) {
      const needle = fv.trim().toLowerCase();
      if (!needle) continue;
      list = list.filter((r) => (r.cells[colId] ?? "").toLowerCase().includes(needle));
    }
    return list;
  }, [rows, query, columnFilters]);

  const total = totalCount ?? filteredRows.length;
  const pageCount = Math.max(1, Math.ceil(total / pageSize));
  const safePage = Math.min(page, pageCount);
  const sliceStart = (safePage - 1) * pageSize;
  const pageRows =
    totalCount != null
      ? filteredRows
      : filteredRows.slice(sliceStart, sliceStart + pageSize);

  const selectedIds = Object.entries(selected)
    .filter(([, v]) => v)
    .map(([id]) => id);

  const allVisibleSelected =
    pageRows.length > 0 && pageRows.every((r) => selected[r.id]);

  return (
    <section className="rounded-lg border border-border bg-surface shadow-sm">
      <div className="flex flex-col gap-2 border-b border-border px-3 py-2.5 sm:flex-row sm:items-center sm:justify-between">
        <div className="min-w-0">
          {title ? <h2 className="text-sm font-semibold">{title}</h2> : null}
          {limitationsNote ? (
            <p className="text-[11px] text-muted-foreground">{limitationsNote}</p>
          ) : null}
        </div>
        <div className="flex flex-wrap items-center gap-1.5">
          <label className="sr-only" htmlFor="admin-grid-search">
            {copy.search}
          </label>
          <input
            id="admin-grid-search"
            type="search"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={copy.search}
            className="min-h-touch w-full rounded-md border border-border bg-background px-2 text-sm sm:w-44"
          />
          <button
            type="button"
            className="min-h-touch rounded-md border border-border px-2 text-xs hover:bg-surface-muted"
            onClick={() => setShowColumns((v) => !v)}
            aria-expanded={showColumns}
          >
            {copy.columns}
          </button>
          <button
            type="button"
            className="min-h-touch rounded-md border border-border px-2 text-xs hover:bg-surface-muted"
            title={copy.exportNote}
            onClick={() => {
              /* UI surface only */
            }}
          >
            {copy.export}
          </button>
          <button
            type="button"
            className="min-h-touch rounded-md border border-border px-2 text-xs hover:bg-surface-muted"
            title={copy.savedViewsNote}
          >
            {copy.savedViews}
          </button>
          <button
            type="button"
            disabled={selectedIds.length === 0}
            className="min-h-touch rounded-md bg-primary px-2 text-xs font-medium text-primary-foreground disabled:opacity-40"
            onClick={() => setConfirmBulk(true)}
          >
            {copy.bulk}
            {selectedIds.length > 0 ? ` (${selectedIds.length})` : ""}
          </button>
        </div>
      </div>

      {showColumns ? (
        <div className="flex flex-wrap gap-3 border-b border-border bg-surface-muted/50 px-3 py-2 text-xs">
          {columns.map((c) => (
            <label key={c.id} className="inline-flex items-center gap-1.5">
              <input
                type="checkbox"
                checked={visible[c.id] !== false}
                onChange={(e) =>
                  setVisible((prev) => ({ ...prev, [c.id]: e.target.checked }))
                }
              />
              {c.label}
            </label>
          ))}
        </div>
      ) : null}

      {error ? (
        <div role="alert" className="px-3 py-6 text-sm text-destructive">
          {copy.error}: {error}
        </div>
      ) : loading ? (
        <div className="px-3 py-8 text-sm text-muted-foreground">{copy.loading}</div>
      ) : pageRows.length === 0 ? (
        <div className="px-3 py-8 text-center text-sm text-muted-foreground">
          {copy.empty}
        </div>
      ) : (
        <>
          {/* Desktop table */}
          <div className="hidden overflow-x-auto md:block">
            <table className="w-full min-w-[40rem] border-collapse text-start text-sm">
              <thead className="bg-surface-muted/70 text-xs">
                <tr>
                  <th className="w-10 px-2 py-2">
                    <input
                      type="checkbox"
                      aria-label={copy.selectAll}
                      checked={allVisibleSelected}
                      onChange={(e) => {
                        const next = { ...selected };
                        for (const r of pageRows) next[r.id] = e.target.checked;
                        setSelected(next);
                      }}
                    />
                  </th>
                  {visibleColumns.map((c) => (
                    <th key={c.id} className="px-2 py-2 font-medium">
                      <div className="flex flex-col gap-1">
                        {c.sortable ? (
                          <button
                            type="button"
                            className="inline-flex items-center gap-1 text-start hover:underline"
                            onClick={() => onSortChange?.(c.id)}
                          >
                            {c.label}
                            {sortColumnId === c.id ? (sortDir === "asc" ? " ↑" : " ↓") : ""}
                          </button>
                        ) : (
                          <span>{c.label}</span>
                        )}
                        {c.filterable ? (
                          <input
                            type="search"
                            aria-label={`${copy.filter} ${c.label}`}
                            value={columnFilters[c.id] ?? ""}
                            onChange={(e) =>
                              setColumnFilters((prev) => ({
                                ...prev,
                                [c.id]: e.target.value,
                              }))
                            }
                            className="min-h-8 rounded border border-border bg-background px-1.5 text-[11px]"
                            placeholder={copy.filter}
                          />
                        ) : null}
                      </div>
                    </th>
                  ))}
                  <th className="px-2 py-2 font-medium">{copy.actions}</th>
                </tr>
              </thead>
              <tbody>
                {pageRows.map((r) => (
                  <tr key={r.id} className="border-t border-border hover:bg-surface-muted/40">
                    <td className="px-2 py-2">
                      <input
                        type="checkbox"
                        checked={!!selected[r.id]}
                        onChange={(e) =>
                          setSelected((prev) => ({ ...prev, [r.id]: e.target.checked }))
                        }
                        aria-label={r.id}
                      />
                    </td>
                    {visibleColumns.map((c) => (
                      <td key={c.id} className="max-w-[14rem] truncate px-2 py-2">
                        {r.cells[c.id] ?? "—"}
                        {r.patternOnly && c.id === visibleColumns[0]?.id ? (
                          <span className="ms-2 rounded bg-accent/20 px-1.5 py-0.5 text-[10px] text-foreground">
                            {copy.patternBadge}
                          </span>
                        ) : null}
                      </td>
                    ))}
                    <td className="px-2 py-2">
                      <button
                        type="button"
                        className="min-h-touch rounded-md border border-border px-2 text-xs hover:bg-surface-muted"
                      >
                        {copy.open}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Mobile operational cards */}
          <ul className="flex flex-col gap-2 p-3 md:hidden">
            {pageRows.map((r) => (
              <li
                key={r.id}
                className="rounded-md border border-border bg-background p-3 shadow-sm"
              >
                <div className="mb-2 flex items-start justify-between gap-2">
                  <label className="inline-flex items-center gap-2 text-sm font-medium">
                    <input
                      type="checkbox"
                      checked={!!selected[r.id]}
                      onChange={(e) =>
                        setSelected((prev) => ({ ...prev, [r.id]: e.target.checked }))
                      }
                    />
                    {r.cells[visibleColumns[0]?.id ?? ""] ?? r.id}
                  </label>
                  <button
                    type="button"
                    className="min-h-touch rounded-md border border-border px-2 text-xs"
                  >
                    {copy.open}
                  </button>
                </div>
                {r.patternOnly ? (
                  <p className="mb-2 text-[10px] text-muted-foreground">{copy.patternBadge}</p>
                ) : null}
                <dl className="grid grid-cols-1 gap-1 text-xs">
                  {visibleColumns.slice(1).map((c) => (
                    <div key={c.id} className="flex justify-between gap-2">
                      <dt className="text-muted-foreground">{c.label}</dt>
                      <dd className="truncate font-medium">{r.cells[c.id] ?? "—"}</dd>
                    </div>
                  ))}
                </dl>
              </li>
            ))}
          </ul>
        </>
      )}

      <div className="flex flex-wrap items-center justify-between gap-2 border-t border-border px-3 py-2 text-xs text-muted-foreground">
        <span>
          {selectedIds.length > 0
            ? `${selectedIds.length} ${copy.selected}`
            : `${copy.page} ${safePage} / ${pageCount}`}
        </span>
        <div className="flex gap-1">
          <button
            type="button"
            className="min-h-touch rounded-md border border-border px-2 disabled:opacity-40"
            disabled={safePage <= 1}
            onClick={() => onPageChange?.(safePage - 1)}
          >
            {copy.prev}
          </button>
          <button
            type="button"
            className="min-h-touch rounded-md border border-border px-2 disabled:opacity-40"
            disabled={safePage >= pageCount}
            onClick={() => onPageChange?.(safePage + 1)}
          >
            {copy.next}
          </button>
        </div>
      </div>

      {confirmBulk ? (
        <div
          role="dialog"
          aria-modal="true"
          aria-labelledby="admin-bulk-confirm-title"
          className="fixed inset-0 z-50 flex items-end justify-center bg-foreground/40 p-3 sm:items-center"
        >
          <div className="w-full max-w-md rounded-lg border border-border bg-surface p-4 shadow-lg">
            <h3 id="admin-bulk-confirm-title" className="text-sm font-semibold">
              {copy.confirmTitle}
            </h3>
            <p className="mt-2 text-sm text-muted-foreground">{copy.confirmBody}</p>
            <div className="mt-4 flex justify-end gap-2">
              <button
                type="button"
                className="min-h-touch rounded-md border border-border px-3 text-sm"
                onClick={() => setConfirmBulk(false)}
              >
                {copy.cancel}
              </button>
              <button
                type="button"
                className="min-h-touch rounded-md bg-primary px-3 text-sm text-primary-foreground"
                onClick={() => setConfirmBulk(false)}
              >
                {copy.confirmOk}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </section>
  );
}
