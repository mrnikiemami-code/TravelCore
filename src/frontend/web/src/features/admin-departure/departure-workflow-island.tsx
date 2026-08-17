"use client";

import { useId, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import {
  createTourDepartureAction,
  getTourDepartureAction,
  listTourDeparturesAction,
  setTourDepartureCapacityAction,
  setTourDepartureScheduleAction,
  setTourDepartureStatusAction,
} from "@/features/admin-departure/actions";
import { getAdminDepartureWorkflowCopy } from "@/features/admin-departure/copy";
import type { TourDepartureDetailView } from "@/features/admin-departure/types";

export type DepartureWorkflowIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

const STATUS_OPTIONS = [
  "Draft",
  "Published",
  "Closed",
  "Cancelled",
  "Completed",
] as const;

export function DepartureWorkflowIsland({
  locale,
  apiConfigured,
}: DepartureWorkflowIslandProps) {
  const copy = getAdminDepartureWorkflowCopy(locale);
  const formId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [productFilter, setProductFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [take, setTake] = useState(50);
  const [items, setItems] = useState<TourDepartureDetailView[]>([]);
  const [detail, setDetail] = useState<TourDepartureDetailView | null>(null);
  const [createProductId, setCreateProductId] = useState("");
  const [status, setStatus] = useState<(typeof STATUS_OPTIONS)[number]>("Draft");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [timeZoneId, setTimeZoneId] = useState("Asia/Tehran");
  const [minPax, setMinPax] = useState("1");
  const [maxPax, setMaxPax] = useState("20");

  function run(job: () => Promise<void>) {
    setError(null);
    startTransition(() => {
      void (async () => {
        try {
          await job();
        } catch (e) {
          setError(e instanceof Error ? e.message : String(e));
        }
      })();
    });
  }

  function applyDetail(item: TourDepartureDetailView) {
    setDetail(item);
    setStatus(
      (STATUS_OPTIONS.find((s) => s === item.status) ?? "Draft") as
        (typeof STATUS_OPTIONS)[number],
    );
    setStartDate(item.startDate ?? "");
    setEndDate(item.endDate ?? "");
    setTimeZoneId(item.timeZoneId ?? "Asia/Tehran");
    setMinPax(item.minimumPax != null ? String(item.minimumPax) : "1");
    setMaxPax(item.maximumPax != null ? String(item.maximumPax) : "20");
  }

  return (
    <Stack gap="md">
      <Text role="muted">{copy.pageIntro}</Text>
      {!apiConfigured ? <Text role="caption">{copy.apiMissing}</Text> : null}
      {error ? (
        <Text role="caption">
          {copy.errorPrefix} {error}
        </Text>
      ) : null}
      {pending ? <Text role="caption">{copy.busy}</Text> : null}

      <Surface className="flex flex-col gap-3 p-4">
        <Text as="h2" role="heading">
          {copy.stepCreate}
        </Text>
        <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-create-product`}>
          {copy.tourProductIdLabel}
          <LtrValue>
            <input
              className="min-h-touch rounded-md border border-border px-3"
              id={`${formId}-create-product`}
              value={createProductId}
              onChange={(e) => setCreateProductId(e.target.value)}
            />
          </LtrValue>
        </label>
        <button
          className="min-h-touch w-fit rounded-md bg-foreground px-4 text-background disabled:opacity-50"
          type="button"
          disabled={!apiConfigured || pending || !createProductId.trim()}
          onClick={() =>
            run(async () => {
              const result = await createTourDepartureAction({
                tourProductId: createProductId.trim(),
              });
              if (!result.ok) throw new Error(result.message);
              applyDetail(result.item);
              setItems((prev) => [result.item, ...prev.filter((x) => x.id !== result.item.id)]);
            })
          }
        >
          {copy.createAction}
        </button>
      </Surface>

      <Surface className="flex flex-col gap-3 p-4">
        <Text as="h2" role="heading">
          {copy.stepBrowse}
        </Text>
        <div className="flex flex-wrap gap-3">
          <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-filter-product`}>
            {copy.productFilterLabel}
            <LtrValue>
              <input
                className="min-h-touch rounded-md border border-border px-3"
                id={`${formId}-filter-product`}
                value={productFilter}
                onChange={(e) => setProductFilter(e.target.value)}
              />
            </LtrValue>
          </label>
          <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-filter-status`}>
            {copy.statusFilterLabel}
            <select
              className="min-h-touch rounded-md border border-border px-3"
              id={`${formId}-filter-status`}
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
            >
              <option value="">{copy.statusAll}</option>
              {STATUS_OPTIONS.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-take`}>
            {copy.takeLabel}
            <LtrValue>
              <input
                className="min-h-touch w-24 rounded-md border border-border px-3"
                id={`${formId}-take`}
                type="number"
                min={1}
                max={200}
                value={take}
                onChange={(e) => setTake(Number(e.target.value) || 50)}
              />
            </LtrValue>
          </label>
        </div>
        <button
          className="min-h-touch w-fit rounded-md border border-border px-4 disabled:opacity-50"
          type="button"
          disabled={!apiConfigured || pending}
          onClick={() =>
            run(async () => {
              const result = await listTourDeparturesAction({
                tourProductId: productFilter.trim() || undefined,
                status: statusFilter || undefined,
                take,
              });
              if (!result.ok) throw new Error(result.message);
              setItems(result.items);
            })
          }
        >
          {copy.refreshList}
        </button>
        {items.length === 0 ? (
          <Text role="muted">{copy.noDepartures}</Text>
        ) : (
          <ul className="flex flex-col gap-2 text-sm">
            {items.map((item) => (
              <li
                key={item.id}
                className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border px-3 py-2"
              >
                <LtrValue>
                  <span>
                    {item.id} · {item.status} · product {item.tourProductId}
                  </span>
                </LtrValue>
                <button
                  className="min-h-touch rounded-md border border-border px-3"
                  type="button"
                  disabled={pending}
                  onClick={() =>
                    run(async () => {
                      const result = await getTourDepartureAction(item.id);
                      if (!result.ok) throw new Error(result.message);
                      applyDetail(result.item);
                    })
                  }
                >
                  {copy.selectDeparture}
                </button>
              </li>
            ))}
          </ul>
        )}
      </Surface>

      {detail ? (
        <Surface className="flex flex-col gap-4 p-4">
          <Text as="h2" role="heading">
            {copy.selectedTitle}
          </Text>
          <LtrValue>
            <Text role="caption">
              {detail.id} · product {detail.tourProductId} · {detail.status}
            </Text>
          </LtrValue>

          <div className="flex flex-col gap-2">
            <Text as="h3" role="heading">
              {copy.scheduleHeading}
            </Text>
            <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-start`}>
              {copy.startDateLabel}
              <LtrValue>
                <input
                  className="min-h-touch rounded-md border border-border px-3"
                  id={`${formId}-start`}
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                />
              </LtrValue>
            </label>
            <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-end`}>
              {copy.endDateLabel}
              <LtrValue>
                <input
                  className="min-h-touch rounded-md border border-border px-3"
                  id={`${formId}-end`}
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                />
              </LtrValue>
            </label>
            <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-tz`}>
              {copy.timeZoneLabel}
              <LtrValue>
                <input
                  className="min-h-touch rounded-md border border-border px-3"
                  id={`${formId}-tz`}
                  value={timeZoneId}
                  onChange={(e) => setTimeZoneId(e.target.value)}
                />
              </LtrValue>
            </label>
            <button
              className="min-h-touch w-fit rounded-md border border-border px-4 disabled:opacity-50"
              type="button"
              disabled={pending}
              onClick={() =>
                run(async () => {
                  const result = await setTourDepartureScheduleAction({
                    id: detail.id,
                    startDate: startDate.trim(),
                    endDate: endDate.trim(),
                    timeZoneId: timeZoneId.trim(),
                  });
                  if (!result.ok) throw new Error(result.message);
                  applyDetail(result.item);
                })
              }
            >
              {copy.saveSchedule}
            </button>
          </div>

          <div className="flex flex-col gap-2">
            <Text as="h3" role="heading">
              {copy.capacityHeading}
            </Text>
            <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-min`}>
              {copy.minPaxLabel}
              <LtrValue>
                <input
                  className="min-h-touch rounded-md border border-border px-3"
                  id={`${formId}-min`}
                  type="number"
                  value={minPax}
                  onChange={(e) => setMinPax(e.target.value)}
                />
              </LtrValue>
            </label>
            <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-max`}>
              {copy.maxPaxLabel}
              <LtrValue>
                <input
                  className="min-h-touch rounded-md border border-border px-3"
                  id={`${formId}-max`}
                  type="number"
                  value={maxPax}
                  onChange={(e) => setMaxPax(e.target.value)}
                />
              </LtrValue>
            </label>
            <button
              className="min-h-touch w-fit rounded-md border border-border px-4 disabled:opacity-50"
              type="button"
              disabled={pending}
              onClick={() =>
                run(async () => {
                  const result = await setTourDepartureCapacityAction({
                    id: detail.id,
                    minimumPax: Number(minPax),
                    maximumPax: Number(maxPax),
                  });
                  if (!result.ok) throw new Error(result.message);
                  applyDetail(result.item);
                })
              }
            >
              {copy.saveCapacity}
            </button>
          </div>

          <div className="flex flex-col gap-2">
            <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-status`}>
              {copy.statusLabel}
              <select
                className="min-h-touch rounded-md border border-border px-3"
                id={`${formId}-status`}
                value={status}
                onChange={(e) =>
                  setStatus(e.target.value as (typeof STATUS_OPTIONS)[number])
                }
              >
                {STATUS_OPTIONS.map((s) => (
                  <option key={s} value={s}>
                    {s}
                  </option>
                ))}
              </select>
            </label>
            <button
              className="min-h-touch w-fit rounded-md border border-border px-4 disabled:opacity-50"
              type="button"
              disabled={pending}
              onClick={() =>
                run(async () => {
                  const result = await setTourDepartureStatusAction({
                    id: detail.id,
                    status,
                  });
                  if (!result.ok) throw new Error(result.message);
                  applyDetail(result.item);
                })
              }
            >
              {copy.saveStatus}
            </button>
          </div>
        </Surface>
      ) : (
        <Text role="muted">{copy.stepInspect}</Text>
      )}
    </Stack>
  );
}
