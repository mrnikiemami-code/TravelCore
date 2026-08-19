"use client";

import { useId, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import {
  approveTravelogueAction,
  listPendingTraveloguesAction,
  publishTravelogueAction,
  rejectTravelogueAction,
} from "@/features/admin-ugc-moderation/actions";
import { getAdminUgcModerationCopy } from "@/features/admin-ugc-moderation/copy";
import type { ModerationQueueTravelogueView } from "@/features/admin-ugc-moderation/types";

export type UgcModerationWorkflowIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

export function UgcModerationWorkflowIsland({
  locale,
  apiConfigured,
}: UgcModerationWorkflowIslandProps) {
  const copy = getAdminUgcModerationCopy(locale);
  const formId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [take, setTake] = useState(50);
  const [items, setItems] = useState<ModerationQueueTravelogueView[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const selected = items.find((x) => x.travelogueId === selectedId) ?? null;

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

  function applyItem(item: ModerationQueueTravelogueView) {
    setSelectedId(item.travelogueId);
    setItems((prev) => {
      const without = prev.filter((x) => x.travelogueId !== item.travelogueId);
      if (item.moderationStatus === "Pending") {
        return [item, ...without];
      }
      return without;
    });
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
                const result = await listPendingTraveloguesAction({ take });
                if (!result.ok) {
                  throw new Error(mapAuthError(result.status) ?? result.message);
                }
                setItems(result.items);
                if (result.items.length === 0) {
                  setSelectedId(null);
                } else if (!selectedId || !result.items.some((x) => x.travelogueId === selectedId)) {
                  setSelectedId(result.items[0]?.travelogueId ?? null);
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
              <li key={item.travelogueId}>
                <button
                  className={`min-h-touch w-full rounded-md border px-3 py-2 text-start ${
                    selectedId === item.travelogueId
                      ? "border-foreground"
                      : "border-border"
                  }`}
                  type="button"
                  onClick={() => setSelectedId(item.travelogueId)}
                >
                  <LtrValue>{item.title}</LtrValue>
                  <Text role="caption">
                    {item.moderationStatus} · {item.publicationStatus} · {item.localeCode}
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
              <LtrValue>{selected.title}</LtrValue>
            </Text>
            <Text role="caption">
              {copy.moderationStatusLabel}: {selected.moderationStatus}
            </Text>
            <Text role="caption">
              {copy.publicationStatusLabel}: {selected.publicationStatus}
            </Text>
            <Text role="caption">
              {copy.localeLabel}: {selected.localeCode}
            </Text>
            <Text role="caption">
              {copy.actorLabel}: <LtrValue>{selected.actorId}</LtrValue>
            </Text>
            <Text role="muted">{copy.bodyPreviewLabel}</Text>
            <Text>{selected.bodyPreview}</Text>
            <div className="flex flex-wrap gap-2">
              <button
                className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                type="button"
                disabled={!apiConfigured || pending || selected.moderationStatus !== "Pending"}
                onClick={() =>
                  run(async () => {
                    const result = await approveTravelogueAction(selected.travelogueId);
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
                disabled={!apiConfigured || pending || selected.moderationStatus !== "Pending"}
                onClick={() =>
                  run(async () => {
                    const result = await rejectTravelogueAction(selected.travelogueId);
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
                  !apiConfigured
                  || pending
                  || selected.moderationStatus !== "Approved"
                  || selected.publicationStatus === "Published"
                }
                onClick={() =>
                  run(async () => {
                    const result = await publishTravelogueAction(selected.travelogueId);
                    if (!result.ok) {
                      throw new Error(mapAuthError(result.status) ?? result.message);
                    }
                    applyItem(result.item);
                  })
                }
              >
                {copy.publishAction}
              </button>
            </div>
          </>
        )}
      </Surface>
    </Stack>
  );
}
