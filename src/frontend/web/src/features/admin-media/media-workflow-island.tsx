"use client";

import { useId, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import {
  generateMediaVariantsAction,
  listMediaAssetsAction,
  loadMediaAssetDetailAction,
  setMediaFocalPointAction,
  uploadMediaAssetAction,
  upsertMediaTranslationAction,
} from "@/features/admin-media/actions";
import { getAdminMediaWorkflowCopy } from "@/features/admin-media/copy";
import type {
  MediaAssetDetailView,
  MediaAssetSummaryView,
} from "@/features/admin-media/types";
import {
  mediaOriginalContentPath,
  resolveMediaAppProxySrc,
} from "@/lib/media/media-presentation";

export type MediaWorkflowIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

function formatBytes(n: number): string {
  if (n < 1024) return `${n} B`;
  if (n < 1024 * 1024) return `${(n / 1024).toFixed(1)} KB`;
  return `${(n / (1024 * 1024)).toFixed(1)} MB`;
}

export function MediaWorkflowIsland({
  locale,
  apiConfigured,
}: MediaWorkflowIslandProps) {
  const copy = getAdminMediaWorkflowCopy(locale);
  const formId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState("");
  const [take, setTake] = useState(50);
  const [items, setItems] = useState<MediaAssetSummaryView[]>([]);
  const [detail, setDetail] = useState<MediaAssetDetailView | null>(null);
  const [translationLocale, setTranslationLocale] = useState<"fa" | "en">(
    locale === "en" ? "en" : "fa",
  );
  const [altText, setAltText] = useState("");
  const [caption, setCaption] = useState("");
  const [publicationStatus, setPublicationStatus] = useState("Draft");
  const [focalX, setFocalX] = useState("");
  const [focalY, setFocalY] = useState("");

  function mapAuthError(status?: number, message?: string) {
    if (status === 401 || status === 403) return copy.unauthorizedBody;
    if (
      status === 400 &&
      message &&
      /svg/i.test(message)
    ) {
      return copy.svgDeniedHint;
    }
    return copy.errorGeneric;
  }

  function run(action: () => Promise<void>) {
    setError(null);
    startTransition(async () => {
      try {
        await action();
      } catch {
        setError(copy.errorGeneric);
      }
    });
  }

  function applyDetail(next: MediaAssetDetailView) {
    setDetail(next);
    const row = next.translations.find(
      (t) => t.localeCode.toLowerCase() === translationLocale,
    );
    setAltText(row?.altText ?? "");
    setCaption(row?.caption ?? "");
    setPublicationStatus(row?.publicationStatus ?? "Draft");
    setFocalX(
      next.asset.focalX === null || next.asset.focalX === undefined
        ? ""
        : String(next.asset.focalX),
    );
    setFocalY(
      next.asset.focalY === null || next.asset.focalY === undefined
        ? ""
        : String(next.asset.focalY),
    );
  }

  async function refreshList() {
    const result = await listMediaAssetsAction({
      status: statusFilter || undefined,
      take,
    });
    if (!result.ok) {
      setError(mapAuthError(result.status, result.message));
      return;
    }
    setItems(result.items);
  }

  async function openAsset(id: string) {
    const result = await loadMediaAssetDetailAction(id);
    if (!result.ok) {
      setError(mapAuthError(result.status, result.message));
      setDetail(null);
      return;
    }
    applyDetail(result.detail);
  }

  if (!apiConfigured) {
    return (
      <Surface tone="muted">
        <Text role="muted">{copy.apiMissing}</Text>
      </Surface>
    );
  }

  const selected = detail?.asset ?? null;
  const previewSrc =
    selected?.status === "Ready"
      ? resolveMediaAppProxySrc(mediaOriginalContentPath(selected.id))
      : null;

  return (
    <Stack gap="lg">
      {error ? (
        <div role="alert">
          <Surface tone="muted">
            <Text role="muted">{error}</Text>
          </Surface>
        </div>
      ) : null}

      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.stepUpload}
          </Text>
          <Text role="caption">{copy.uploadHint}</Text>
          <form
            id={`${formId}-upload`}
            className="flex flex-col gap-3 sm:flex-row sm:items-end"
            onSubmit={(e) => {
              e.preventDefault();
              const fd = new FormData(e.currentTarget);
              run(async () => {
                const result = await uploadMediaAssetAction(fd);
                if (!result.ok) {
                  setError(mapAuthError(result.status, result.message));
                  return;
                }
                e.currentTarget.reset();
                await refreshList();
                await openAsset(result.asset.id);
              });
            }}
          >
            <label className="flex min-w-0 flex-1 flex-col gap-1 text-sm">
              <span>{copy.fileLabel}</span>
              <input
                name="file"
                type="file"
                accept="image/png,image/jpeg,image/webp,image/gif,.png,.jpg,.jpeg,.webp"
                required
                className="min-h-touch rounded-md border border-border bg-background px-3 py-2"
              />
            </label>
            <button
              type="submit"
              disabled={pending}
              className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
            >
              {copy.uploadAction}
            </button>
          </form>
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.stepBrowse}
          </Text>
          <form
            className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end"
            onSubmit={(e) => {
              e.preventDefault();
              run(async () => {
                await refreshList();
              });
            }}
          >
            <label className="flex flex-col gap-1 text-sm">
              <span>{copy.statusFilterLabel}</span>
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                className="min-h-touch rounded-md border border-border bg-background px-3"
              >
                <option value="">{copy.statusAll}</option>
                <option value="Ready">{copy.statusReady}</option>
                <option value="PendingStorage">{copy.statusPending}</option>
                <option value="Failed">{copy.statusFailed}</option>
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span>{copy.takeLabel}</span>
              <input
                type="number"
                min={1}
                max={200}
                value={take}
                onChange={(e) => setTake(Number(e.target.value) || 50)}
                className="min-h-touch w-28 rounded-md border border-border bg-background px-3"
              />
            </label>
            <button
              type="submit"
              disabled={pending}
              className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
            >
              {copy.refreshList}
            </button>
          </form>

          {items.length === 0 ? (
            <Text role="caption">{copy.noAssets}</Text>
          ) : (
            <ul className="flex flex-col gap-2">
              {items.map((item) => (
                <li
                  key={item.id}
                  className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border px-3 py-2"
                >
                  <div className="min-w-0">
                    <Text as="p">
                      <LtrValue>{item.contentType}</LtrValue>
                      {" · "}
                      {item.status}
                      {item.width && item.height ? (
                        <>
                          {" · "}
                          <LtrValue>
                            {item.width}×{item.height}
                          </LtrValue>
                        </>
                      ) : null}
                    </Text>
                    <Text role="caption">
                      {formatBytes(item.byteSize)}
                      {" · "}
                      <LtrValue>{item.createdAt}</LtrValue>
                    </Text>
                  </div>
                  <button
                    type="button"
                    disabled={pending}
                    className="min-h-touch rounded-md border border-border px-3 disabled:opacity-50"
                    onClick={() =>
                      run(async () => {
                        await openAsset(item.id);
                      })
                    }
                  >
                    {copy.selectAsset}
                  </button>
                </li>
              ))}
            </ul>
          )}
        </Stack>
      </Surface>

      {selected ? (
        <>
          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.stepInspect}
              </Text>
              <Text role="muted">{copy.selectedTitle}</Text>
              <div className="grid gap-3 sm:grid-cols-2">
                <div>
                  <Text as="h3" role="heading">
                    {copy.metadataHeading}
                  </Text>
                  <dl className="mt-2 flex flex-col gap-1 text-sm">
                    <div>
                      <dt className="inline text-muted-foreground">
                        {copy.contentTypeLabel}:{" "}
                      </dt>
                      <dd className="inline">
                        <LtrValue>{selected.contentType}</LtrValue>
                      </dd>
                    </div>
                    <div>
                      <dt className="inline text-muted-foreground">
                        {copy.dimensionsLabel}:{" "}
                      </dt>
                      <dd className="inline">
                        {selected.width && selected.height ? (
                          <LtrValue>
                            {selected.width}×{selected.height}
                          </LtrValue>
                        ) : (
                          "—"
                        )}
                      </dd>
                    </div>
                    <div>
                      <dt className="inline text-muted-foreground">
                        {copy.byteSizeLabel}:{" "}
                      </dt>
                      <dd className="inline">{formatBytes(selected.byteSize)}</dd>
                    </div>
                    <div>
                      <dt className="inline text-muted-foreground">
                        {copy.statusLabel}:{" "}
                      </dt>
                      <dd className="inline">{selected.status}</dd>
                    </div>
                    <div>
                      <dt className="inline text-muted-foreground">
                        {copy.createdLabel}:{" "}
                      </dt>
                      <dd className="inline">
                        <LtrValue>{selected.createdAt}</LtrValue>
                      </dd>
                    </div>
                  </dl>
                </div>
                <div>
                  <Text as="h3" role="heading">
                    {copy.previewLabel}
                  </Text>
                  {previewSrc ? (
                    <button
                      type="button"
                      className="relative mt-2 block max-w-full overflow-hidden rounded-md border border-border focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2"
                      aria-label={copy.pickFocalOnPreview}
                      disabled={pending}
                      onClick={(e) => {
                        const rect = e.currentTarget.getBoundingClientRect();
                        const x = (e.clientX - rect.left) / rect.width;
                        const y = (e.clientY - rect.top) / rect.height;
                        const nx = Math.min(1, Math.max(0, x));
                        const ny = Math.min(1, Math.max(0, y));
                        setFocalX(nx.toFixed(4));
                        setFocalY(ny.toFixed(4));
                      }}
                    >
                      {/* eslint-disable-next-line @next/next/no-img-element -- Admin app-proxy preview; not SEO LCP path */}
                      <img
                        src={previewSrc}
                        alt={altText || copy.previewLabel}
                        className="max-h-72 w-full object-contain"
                      />
                      {selected.focalX !== null &&
                      selected.focalY !== null ? (
                        <span
                          aria-hidden
                          className="pointer-events-none absolute h-3 w-3 -translate-x-1/2 -translate-y-1/2 rounded-full border-2 border-background bg-foreground"
                          style={{
                            left: `${selected.focalX * 100}%`,
                            top: `${selected.focalY * 100}%`,
                          }}
                        />
                      ) : null}
                    </button>
                  ) : (
                    <Text role="caption">{copy.previewUnavailable}</Text>
                  )}
                </div>
              </div>

              <div className="border-t border-border pt-3">
                <Text as="h3" role="heading">
                  {copy.variantsHeading}
                </Text>
                <button
                  type="button"
                  disabled={pending || selected.status !== "Ready"}
                  className="mt-2 min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                  onClick={() =>
                    run(async () => {
                      const result = await generateMediaVariantsAction(
                        selected.id,
                      );
                      if (!result.ok) {
                        setError(mapAuthError(result.status, result.message));
                        return;
                      }
                      await openAsset(selected.id);
                    })
                  }
                >
                  {copy.generateVariants}
                </button>
                {!detail?.variants.length ? (
                  <Text role="caption">{copy.noVariants}</Text>
                ) : (
                  <ul className="mt-2 flex flex-col gap-2">
                    {detail.variants.map((v) => (
                      <li
                        key={v.id}
                        className="rounded-md border border-border px-3 py-2 text-sm"
                      >
                        <Text as="p">
                          {copy.variantProfile}:{" "}
                          <LtrValue>{v.profile}</LtrValue>
                          {" · "}
                          {copy.variantStatus}: {v.status}
                          {v.width && v.height ? (
                            <>
                              {" · "}
                              <LtrValue>
                                {v.width}×{v.height}
                              </LtrValue>
                            </>
                          ) : null}
                        </Text>
                        {v.failureReason ? (
                          <Text role="caption">
                            {copy.variantFailure}: {v.failureReason}
                          </Text>
                        ) : null}
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.stepTranslate}
              </Text>
              <form
                className="flex flex-col gap-3"
                onSubmit={(e) => {
                  e.preventDefault();
                  run(async () => {
                    const result = await upsertMediaTranslationAction({
                      mediaAssetId: selected.id,
                      localeCode: translationLocale,
                      altText,
                      caption: caption || null,
                      publicationStatus,
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status, result.message));
                      return;
                    }
                    await openAsset(selected.id);
                  });
                }}
              >
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.translationLocale}</span>
                  <select
                    value={translationLocale}
                    onChange={(e) => {
                      const next = e.target.value as "fa" | "en";
                      setTranslationLocale(next);
                      const row = detail?.translations.find(
                        (t) => t.localeCode.toLowerCase() === next,
                      );
                      setAltText(row?.altText ?? "");
                      setCaption(row?.caption ?? "");
                      setPublicationStatus(row?.publicationStatus ?? "Draft");
                    }}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  >
                    <option value="fa">fa</option>
                    <option value="en">en</option>
                  </select>
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.altLabel}</span>
                  <input
                    value={altText}
                    onChange={(e) => setAltText(e.target.value)}
                    required
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.captionLabel}</span>
                  <textarea
                    value={caption}
                    onChange={(e) => setCaption(e.target.value)}
                    rows={2}
                    className="rounded-md border border-border bg-background px-3 py-2"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.publicationLabel}</span>
                  <select
                    value={publicationStatus}
                    onChange={(e) => setPublicationStatus(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  >
                    <option value="Draft">{copy.publicationDraft}</option>
                    <option value="Ready">{copy.publicationReady}</option>
                    <option value="Published">{copy.publicationPublished}</option>
                    <option value="Archived">{copy.publicationArchived}</option>
                  </select>
                </label>
                <button
                  type="submit"
                  disabled={pending}
                  className="min-h-touch w-fit rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                >
                  {copy.saveTranslation}
                </button>
              </form>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.stepFocal}
              </Text>
              <Text role="caption">{copy.focalHint}</Text>
              <form
                className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end"
                onSubmit={(e) => {
                  e.preventDefault();
                  const xRaw = focalX.trim();
                  const yRaw = focalY.trim();
                  run(async () => {
                    const result = await setMediaFocalPointAction({
                      mediaAssetId: selected.id,
                      focalX: xRaw === "" ? null : Number(xRaw),
                      focalY: yRaw === "" ? null : Number(yRaw),
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status, result.message));
                      return;
                    }
                    await openAsset(selected.id);
                  });
                }}
              >
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.focalXLabel}</span>
                  <input
                    type="number"
                    step="0.0001"
                    min={0}
                    max={1}
                    value={focalX}
                    onChange={(e) => setFocalX(e.target.value)}
                    className="min-h-touch w-36 rounded-md border border-border bg-background px-3"
                    dir="ltr"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.focalYLabel}</span>
                  <input
                    type="number"
                    step="0.0001"
                    min={0}
                    max={1}
                    value={focalY}
                    onChange={(e) => setFocalY(e.target.value)}
                    className="min-h-touch w-36 rounded-md border border-border bg-background px-3"
                    dir="ltr"
                  />
                </label>
                <button
                  type="submit"
                  disabled={pending}
                  className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                >
                  {copy.saveFocal}
                </button>
                <button
                  type="button"
                  disabled={pending}
                  className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                  onClick={() =>
                    run(async () => {
                      const result = await setMediaFocalPointAction({
                        mediaAssetId: selected.id,
                        focalX: null,
                        focalY: null,
                      });
                      if (!result.ok) {
                        setError(mapAuthError(result.status, result.message));
                        return;
                      }
                      setFocalX("");
                      setFocalY("");
                      await openAsset(selected.id);
                    })
                  }
                >
                  {copy.clearFocal}
                </button>
              </form>
            </Stack>
          </Surface>
        </>
      ) : null}
    </Stack>
  );
}
