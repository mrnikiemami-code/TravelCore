"use client";

import { useId, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import {
  approveAgencyOfferAction,
  listPendingAgencyOffersAction,
  rejectAgencyOfferAction,
  suspendAgencyOfferAction,
} from "@/features/admin-agency-offer-governance/actions";
import { getAdminAgencyOfferGovernanceCopy } from "@/features/admin-agency-offer-governance/copy";
import type { AgencyOfferModerationQueueView } from "@/features/admin-agency-offer-governance/types";

export type AgencyOfferGovernanceWorkflowIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

export function AgencyOfferGovernanceWorkflowIsland({
  locale,
  apiConfigured,
}: AgencyOfferGovernanceWorkflowIslandProps) {
  const copy = getAdminAgencyOfferGovernanceCopy(locale);
  const formId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [take, setTake] = useState(50);
  const [items, setItems] = useState<AgencyOfferModerationQueueView[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const selected = items.find((x) => x.offerId === selectedId) ?? null;

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

  function mapAuthError(status?: number) {
    if (status === 401 || status === 403) {
      return copy.authRequired;
    }
    return null;
  }

  function applyItem(item: AgencyOfferModerationQueueView) {
    setSelectedId(item.offerId);
    setItems((prev) => {
      const without = prev.filter((x) => x.offerId !== item.offerId);
      if (item.publicationStatus === "Submitted") {
        return [item, ...without];
      }
      return without;
    });
  }

  return (
    <Stack gap="md">
      <Text role="muted">{copy.pageIntro}</Text>
      <Text role="caption">{copy.boundaryNote}</Text>
      {!apiConfigured ? <Text role="caption">{copy.apiMissing}</Text> : null}
      {error ? (
        <Text role="caption">
          {copy.errorPrefix} {error}
        </Text>
      ) : null}
      {pending ? <Text role="caption">{copy.busy}</Text> : null}

      <Surface className="flex flex-col gap-3 p-4">
        <Text as="h2" role="heading">
          {copy.stepQueue}
        </Text>
        <div className="flex flex-wrap items-end gap-3">
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
          <button
            className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
            type="button"
            disabled={!apiConfigured || pending}
            onClick={() =>
              run(async () => {
                const result = await listPendingAgencyOffersAction({ take });
                if (!result.ok) {
                  throw new Error(mapAuthError(result.status) ?? result.message);
                }
                setItems(result.items);
                if (result.items.length === 0) {
                  setSelectedId(null);
                } else if (!selectedId || !result.items.some((x) => x.offerId === selectedId)) {
                  setSelectedId(result.items[0]?.offerId ?? null);
                }
              })
            }
          >
            {copy.refreshQueue}
          </button>
        </div>
        {items.length === 0 ? (
          <Text role="muted">{copy.noItems}</Text>
        ) : (
          <ul className="flex flex-col gap-2">
            {items.map((item) => (
              <li key={item.offerId}>
                <button
                  className={`min-h-touch w-full rounded-md border px-3 py-2 text-start ${
                    selectedId === item.offerId ? "border-foreground" : "border-border"
                  }`}
                  type="button"
                  onClick={() => setSelectedId(item.offerId)}
                >
                  <LtrValue>{item.titleOverride ?? item.offerId}</LtrValue>
                  <Text role="caption">
                    {item.publicationStatus} · {item.salesChannel} · {item.visibility}
                  </Text>
                </button>
              </li>
            ))}
          </ul>
        )}
      </Surface>

      <Surface className="flex flex-col gap-3 p-4">
        {!selected ? (
          <Text role="muted">{copy.selectItem}</Text>
        ) : (
          <>
            <Text as="h2" role="heading">
              <LtrValue>{selected.titleOverride ?? selected.offerId}</LtrValue>
            </Text>
            <Text role="caption">
              {copy.agencyProfileLabel}: <LtrValue>{selected.agencyProfileId}</LtrValue>
            </Text>
            <Text role="caption">
              {copy.tourProductLabel}: <LtrValue>{selected.tourProductId}</LtrValue>
            </Text>
            <Text role="caption">
              {copy.publicationStatusLabel}: {selected.publicationStatus}
            </Text>
            <Text role="caption">
              {copy.visibilityLabel}: {selected.visibility}
            </Text>
            <Text role="caption">
              {copy.salesChannelLabel}: {selected.salesChannel}
            </Text>
            {selected.highlight ? (
              <Text role="caption">
                {copy.highlightLabel}: {selected.highlight}
              </Text>
            ) : null}
            <div className="flex flex-wrap gap-2">
              <button
                className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                type="button"
                disabled={
                  !apiConfigured || pending || selected.publicationStatus !== "Submitted"
                }
                onClick={() =>
                  run(async () => {
                    const result = await approveAgencyOfferAction(selected.offerId);
                    if (!result.ok) {
                      throw new Error(mapAuthError(result.status) ?? result.message);
                    }
                    applyItem(result.item);
                  })
                }
              >
                {copy.approveAction}
              </button>
              <button
                className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                type="button"
                disabled={
                  !apiConfigured || pending || selected.publicationStatus !== "Submitted"
                }
                onClick={() =>
                  run(async () => {
                    const result = await rejectAgencyOfferAction(selected.offerId);
                    if (!result.ok) {
                      throw new Error(mapAuthError(result.status) ?? result.message);
                    }
                    applyItem(result.item);
                  })
                }
              >
                {copy.rejectAction}
              </button>
              <button
                className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                type="button"
                disabled={
                  !apiConfigured || pending || selected.publicationStatus !== "Published"
                }
                onClick={() =>
                  run(async () => {
                    const result = await suspendAgencyOfferAction(selected.offerId);
                    if (!result.ok) {
                      throw new Error(mapAuthError(result.status) ?? result.message);
                    }
                    applyItem(result.item);
                  })
                }
              >
                {copy.suspendAction}
              </button>
            </div>
          </>
        )}
      </Surface>
    </Stack>
  );
}
