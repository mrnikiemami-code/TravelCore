"use client";

import { useId, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import { listMediaAssetsAction } from "@/features/admin-media/actions";
import type { MediaAssetSummaryView } from "@/features/admin-media/types";
import {
  addTourGalleryItemAction,
  createTourProductAction,
  listTourProductsAction,
  loadTourDetailAction,
  loadTourMediaAction,
  openTourByCodeAction,
  publishTourSeoRouteAction,
  removeTourCoverAction,
  setTourCatalogStatusAction,
  setTourClassificationAction,
  setTourCoverAction,
  setTourTranslationSlugAction,
  upsertTourTranslationAction,
} from "@/features/admin-tour/actions";
import { getAdminTourWorkflowCopy } from "@/features/admin-tour/copy";
import type {
  TourMediaView,
  TourProductDetailView,
  TourProductSummaryView,
} from "@/features/admin-tour/types";

export type TourWorkflowIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

export function TourWorkflowIsland({
  locale,
  apiConfigured,
}: TourWorkflowIslandProps) {
  const copy = getAdminTourWorkflowCopy(locale);
  const formId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [kindFilter, setKindFilter] = useState("");
  const [take, setTake] = useState(50);
  const [items, setItems] = useState<TourProductSummaryView[]>([]);
  const [detail, setDetail] = useState<TourProductDetailView | null>(null);
  const [media, setMedia] = useState<TourMediaView | null>(null);
  const [readyMedia, setReadyMedia] = useState<MediaAssetSummaryView[]>([]);
  const [openCode, setOpenCode] = useState("");
  const [createKind, setCreateKind] = useState<"Experience" | "Package">(
    "Experience",
  );
  const [createCode, setCreateCode] = useState("");
  const [createName, setCreateName] = useState("");
  const [translationLocale, setTranslationLocale] = useState<"fa" | "en">(
    locale === "en" ? "en" : "fa",
  );
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [tourSlug, setTourSlug] = useState("");
  const [catalogStatus, setCatalogStatus] = useState<
    "Draft" | "Published" | "Inactive"
  >("Draft");
  const [classification, setClassification] = useState("");
  const [seoPath, setSeoPath] = useState<string | null>(null);

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

  function applyDetail(item: TourProductDetailView) {
    setDetail(item);
    setCatalogStatus(
      (item.catalogStatus as "Draft" | "Published" | "Inactive") || "Draft",
    );
    setClassification(item.classificationCode ?? "");
    setTitle(item.localizedTitle ?? "");
    setDescription(item.localizedDescription ?? "");
    setTourSlug(item.localizedSlug ?? "");
  }

  if (!apiConfigured) {
    return <Text role="muted">{copy.apiMissing}</Text>;
  }

  return (
    <Stack gap="lg">
      {error ? (
        <Text role="muted">
          {copy.errorPrefix}: {error}
        </Text>
      ) : null}
      {pending ? <Text role="caption">{copy.busy}</Text> : null}

      <Surface>
        <Stack gap="md">
          <Text as="h2" role="heading">
            {copy.stepCreate}
          </Text>
          <label className="flex flex-col gap-1 text-sm">
            {copy.kindLabel}
            <select
              className="min-h-touch rounded border px-2"
              value={createKind}
              onChange={(e) =>
                setCreateKind(e.target.value as "Experience" | "Package")
              }
            >
              <option value="Experience">Experience</option>
              <option value="Package">Package</option>
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            {copy.codeLabel}
            <input
              className="min-h-touch rounded border px-2"
              value={createCode}
              onChange={(e) => setCreateCode(e.target.value)}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            {copy.englishNameLabel}
            <input
              className="min-h-touch rounded border px-2"
              value={createName}
              onChange={(e) => setCreateName(e.target.value)}
            />
          </label>
          <button
            type="button"
            className="min-h-touch rounded border px-3"
            disabled={pending}
            onClick={() =>
              run(async () => {
                const created = await createTourProductAction({
                  kind: createKind,
                  code: createCode.trim(),
                  englishName: createName.trim(),
                });
                if (!created.ok) throw new Error(created.message);
                const loaded = await loadTourDetailAction({
                  id: created.item.id,
                  locale: translationLocale,
                });
                if (!loaded.ok) throw new Error(loaded.message);
                applyDetail(loaded.item);
                setCreateCode("");
                setCreateName("");
              })
            }
          >
            {copy.createAction}
          </button>
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="md">
          <Text as="h2" role="heading">
            {copy.stepBrowse}
          </Text>
          <label className="flex flex-col gap-1 text-sm">
            {copy.kindFilterLabel}
            <select
              className="min-h-touch rounded border px-2"
              value={kindFilter}
              onChange={(e) => setKindFilter(e.target.value)}
            >
              <option value="">{copy.kindAll}</option>
              <option value="Experience">Experience</option>
              <option value="Package">Package</option>
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            {copy.takeLabel}
            <input
              type="number"
              className="min-h-touch rounded border px-2"
              value={take}
              onChange={(e) => setTake(Number(e.target.value) || 50)}
            />
          </label>
          <button
            type="button"
            className="min-h-touch rounded border px-3"
            disabled={pending}
            onClick={() =>
              run(async () => {
                const listed = await listTourProductsAction({
                  kind: kindFilter || undefined,
                  take,
                });
                if (!listed.ok) throw new Error(listed.message);
                setItems(listed.items);
              })
            }
          >
            {copy.refreshList}
          </button>
          {items.length === 0 ? (
            <Text role="muted">{copy.noTours}</Text>
          ) : (
            <ul className="flex flex-col gap-2 text-sm">
              {items.map((item) => (
                <li key={item.id} className="flex flex-wrap items-center gap-2">
                  <LtrValue>{item.code}</LtrValue> · {item.kind} ·{" "}
                  {item.catalogStatus}
                  <button
                    type="button"
                    className="min-h-touch rounded border px-2"
                    disabled={pending}
                    onClick={() =>
                      run(async () => {
                        const loaded = await loadTourDetailAction({
                          id: item.id,
                          locale: translationLocale,
                        });
                        if (!loaded.ok) throw new Error(loaded.message);
                        applyDetail(loaded.item);
                        const m = await loadTourMediaAction({ id: item.id });
                        if (m.ok) setMedia(m.media);
                      })
                    }
                  >
                    {copy.selectTour}
                  </button>
                </li>
              ))}
            </ul>
          )}
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="md">
          <Text as="h2" role="heading">
            {copy.stepOpenByCode}
          </Text>
          <Text role="caption">{copy.openByCodeHint}</Text>
          <input
            className="min-h-touch rounded border px-2"
            value={openCode}
            onChange={(e) => setOpenCode(e.target.value)}
          />
          <button
            type="button"
            className="min-h-touch rounded border px-3"
            disabled={pending}
            onClick={() =>
              run(async () => {
                const opened = await openTourByCodeAction({
                  code: openCode.trim(),
                  locale: translationLocale,
                });
                if (!opened.ok) throw new Error(opened.message);
                applyDetail(opened.item);
                const m = await loadTourMediaAction({ id: opened.item.id });
                if (m.ok) setMedia(m.media);
              })
            }
          >
            {copy.openByCodeAction}
          </button>
        </Stack>
      </Surface>

      {detail ? (
        <>
          <Surface>
            <Stack gap="md">
              <Text as="h2" role="heading">
                {copy.stepInspect}
              </Text>
              <Text>
                {copy.selectedTitle}: <LtrValue>{detail.code}</LtrValue> ·{" "}
                {detail.kind} · {detail.englishName}
              </Text>
              <Text role="caption">
                {copy.statusLabel}: {detail.catalogStatus}
              </Text>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="md">
              <Text as="h2" role="heading">
                {copy.stepTranslate}
              </Text>
              <label className="flex flex-col gap-1 text-sm">
                {copy.translationLocale}
                <select
                  className="min-h-touch rounded border px-2"
                  value={translationLocale}
                  onChange={(e) =>
                    setTranslationLocale(e.target.value as "fa" | "en")
                  }
                >
                  <option value="fa">fa</option>
                  <option value="en">en</option>
                </select>
              </label>
              <label className="flex flex-col gap-1 text-sm">
                {copy.titleLabel}
                <input
                  className="min-h-touch rounded border px-2"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                {copy.descriptionLabel}
                <textarea
                  className="min-h-touch rounded border px-2"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                />
              </label>
              <button
                type="button"
                className="min-h-touch rounded border px-3"
                disabled={pending}
                onClick={() =>
                  run(async () => {
                    const saved = await upsertTourTranslationAction({
                      id: detail.id,
                      localeCode: translationLocale,
                      title: title.trim(),
                      description: description.trim() || null,
                    });
                    if (!saved.ok) throw new Error(saved.message);
                    const loaded = await loadTourDetailAction({
                      id: detail.id,
                      locale: translationLocale,
                    });
                    if (!loaded.ok) throw new Error(loaded.message);
                    applyDetail(loaded.item);
                  })
                }
              >
                {copy.saveTranslation}
              </button>
              <label className="flex flex-col gap-1 text-sm">
                {copy.slugLabel}
                <input
                  className="min-h-touch rounded border px-2"
                  value={tourSlug}
                  onChange={(e) => setTourSlug(e.target.value)}
                />
              </label>
              <Text role="caption">{copy.slugHint}</Text>
              <button
                type="button"
                className="min-h-touch rounded border px-3"
                disabled={pending}
                onClick={() =>
                  run(async () => {
                    const saved = await setTourTranslationSlugAction({
                      id: detail.id,
                      localeCode: translationLocale,
                      slug: tourSlug.trim() || null,
                    });
                    if (!saved.ok) throw new Error(saved.message);
                    applyDetail(saved.item);
                  })
                }
              >
                {copy.saveSlug}
              </button>
              <Text role="caption">{copy.publishSeoHint}</Text>
              <button
                type="button"
                className="min-h-touch rounded border px-3"
                disabled={pending || !tourSlug.trim()}
                onClick={() =>
                  run(async () => {
                    const published = await publishTourSeoRouteAction({
                      tourProductId: detail.id,
                      localeCode: translationLocale,
                      slug: tourSlug.trim(),
                    });
                    if (!published.ok) throw new Error(published.message);
                    setSeoPath(published.publicPath);
                  })
                }
              >
                {copy.publishSeoRoute}
              </button>
              {seoPath ? (
                <Text role="caption">
                  SEO: <LtrValue>{seoPath}</LtrValue>
                </Text>
              ) : null}
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="md">
              <Text as="h2" role="heading">
                {copy.stepCatalog}
              </Text>
              <label className="flex flex-col gap-1 text-sm">
                {copy.statusLabel}
                <select
                  className="min-h-touch rounded border px-2"
                  value={catalogStatus}
                  onChange={(e) =>
                    setCatalogStatus(
                      e.target.value as "Draft" | "Published" | "Inactive",
                    )
                  }
                >
                  <option value="Draft">Draft</option>
                  <option value="Published">Published</option>
                  <option value="Inactive">Inactive</option>
                </select>
              </label>
              <button
                type="button"
                className="min-h-touch rounded border px-3"
                disabled={pending}
                onClick={() =>
                  run(async () => {
                    const saved = await setTourCatalogStatusAction({
                      id: detail.id,
                      catalogStatus,
                    });
                    if (!saved.ok) throw new Error(saved.message);
                    applyDetail(saved.item);
                  })
                }
              >
                {copy.saveStatus}
              </button>
              <label className="flex flex-col gap-1 text-sm">
                {copy.classificationLabel}
                <input
                  className="min-h-touch rounded border px-2"
                  value={classification}
                  onChange={(e) => setClassification(e.target.value)}
                />
              </label>
              <button
                type="button"
                className="min-h-touch rounded border px-3"
                disabled={pending}
                onClick={() =>
                  run(async () => {
                    const saved = await setTourClassificationAction({
                      id: detail.id,
                      classificationCode: classification.trim() || null,
                    });
                    if (!saved.ok) throw new Error(saved.message);
                    const loaded = await loadTourDetailAction({
                      id: detail.id,
                      locale: translationLocale,
                    });
                    if (!loaded.ok) throw new Error(loaded.message);
                    applyDetail(loaded.item);
                  })
                }
              >
                {copy.saveClassification}
              </button>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="md">
              <Text as="h2" role="heading">
                {copy.stepMedia}
              </Text>
              <Text role="caption">{copy.mediaReadyHint}</Text>
              <button
                type="button"
                className="min-h-touch rounded border px-3"
                disabled={pending}
                onClick={() =>
                  run(async () => {
                    const listed = await listMediaAssetsAction({
                      status: "Ready",
                      take: 30,
                    });
                    if (!listed.ok) throw new Error(listed.message);
                    setReadyMedia(listed.items);
                    const m = await loadTourMediaAction({ id: detail.id });
                    if (m.ok) setMedia(m.media);
                  })
                }
              >
                {copy.refreshReadyMedia}
              </button>
              {media ? (
                <Text role="caption">
                  Cover:{" "}
                  <LtrValue>{media.coverMediaAssetId ?? "—"}</LtrValue> ·
                  Gallery: {media.galleryMediaAssetIds.length}
                </Text>
              ) : null}
              {media?.coverMediaAssetId ? (
                <button
                  type="button"
                  className="min-h-touch rounded border px-3"
                  disabled={pending}
                  onClick={() =>
                    run(async () => {
                      const removed = await removeTourCoverAction({
                        id: detail.id,
                      });
                      if (!removed.ok) throw new Error(removed.message);
                      setMedia(removed.media);
                    })
                  }
                >
                  {copy.removeCover}
                </button>
              ) : null}
              <ul className="flex flex-col gap-2 text-sm" id={formId}>
                {readyMedia.map((asset) => (
                  <li
                    key={asset.id}
                    className="flex flex-wrap items-center gap-2"
                  >
                    <LtrValue>{asset.id}</LtrValue> · {asset.status}
                    <button
                      type="button"
                      className="min-h-touch rounded border px-2"
                      disabled={pending}
                      onClick={() =>
                        run(async () => {
                          const set = await setTourCoverAction({
                            id: detail.id,
                            mediaAssetId: asset.id,
                          });
                          if (!set.ok) throw new Error(set.message);
                          setMedia(set.media);
                        })
                      }
                    >
                      {copy.setCover}
                    </button>
                    <button
                      type="button"
                      className="min-h-touch rounded border px-2"
                      disabled={pending}
                      onClick={() =>
                        run(async () => {
                          const added = await addTourGalleryItemAction({
                            id: detail.id,
                            mediaAssetId: asset.id,
                          });
                          if (!added.ok) throw new Error(added.message);
                          setMedia(added.media);
                        })
                      }
                    >
                      {copy.addGallery}
                    </button>
                  </li>
                ))}
              </ul>
            </Stack>
          </Surface>
        </>
      ) : null}
    </Stack>
  );
}
