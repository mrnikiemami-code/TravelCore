"use client";

import { useId, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import { listMediaAssetsAction } from "@/features/admin-media/actions";
import type { MediaAssetSummaryView } from "@/features/admin-media/types";
import { resolveDestinationBySlugAction } from "@/features/admin-place/actions";
import {
  addContentHeadingBlockAction,
  addContentImageBlockAction,
  addContentParagraphBlockAction,
  assignContentCategoryAction,
  assignContentDestinationAction,
  assignContentTagAction,
  createContentCategoryAction,
  createContentItemAction,
  createContentTagAction,
  listContentCategoriesAction,
  listContentItemsAction,
  listContentTagsAction,
  loadContentDetailAction,
  openContentByCodeAction,
  removeContentBlockAction,
  removeContentCategoryAction,
  removeContentDestinationAction,
  removeContentTagAction,
  reorderContentBlocksAction,
  setContentTranslationSlugAction,
  publishContentSeoRouteAction,
  upsertContentTranslationAction,
} from "@/features/admin-content/actions";
import { getAdminContentWorkflowCopy } from "@/features/admin-content/copy";
import type {
  ContentCategoryView,
  ContentDetailView,
  ContentItemSummaryView,
  ContentTagView,
} from "@/features/admin-content/types";
import {
  mediaVariantContentPath,
  resolveMediaAppProxySrc,
} from "@/lib/media/media-presentation";

export type ContentWorkflowIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

export function ContentWorkflowIsland({
  locale,
  apiConfigured,
}: ContentWorkflowIslandProps) {
  const copy = getAdminContentWorkflowCopy(locale);
  const formId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [kindFilter, setKindFilter] = useState("");
  const [take, setTake] = useState(50);
  const [items, setItems] = useState<ContentItemSummaryView[]>([]);
  const [detail, setDetail] = useState<ContentDetailView | null>(null);
  const [openCode, setOpenCode] = useState("");
  const [createKind, setCreateKind] = useState<
    "Article" | "LandingPage" | "Guide"
  >("Article");
  const [createCode, setCreateCode] = useState("");
  const [createName, setCreateName] = useState("");
  const [translationLocale, setTranslationLocale] = useState<"fa" | "en">(
    locale === "en" ? "en" : "fa",
  );
  const [title, setTitle] = useState("");
  const [body, setBody] = useState("");
  const [excerpt, setExcerpt] = useState("");
  const [contentSlug, setContentSlug] = useState("");
  const [categories, setCategories] = useState<ContentCategoryView[]>([]);
  const [tags, setTags] = useState<ContentTagView[]>([]);
  const [categoryCode, setCategoryCode] = useState("");
  const [categoryName, setCategoryName] = useState("");
  const [tagCode, setTagCode] = useState("");
  const [tagName, setTagName] = useState("");
  const [selectedCategoryId, setSelectedCategoryId] = useState("");
  const [selectedTagId, setSelectedTagId] = useState("");
  const [headingText, setHeadingText] = useState("");
  const [headingLevel, setHeadingLevel] = useState("2");
  const [paragraphText, setParagraphText] = useState("");
  const [readyMedia, setReadyMedia] = useState<MediaAssetSummaryView[]>([]);
  const [readyMediaLoaded, setReadyMediaLoaded] = useState(false);
  const [destSlugLocale, setDestSlugLocale] = useState<"fa" | "en">(
    locale === "en" ? "en" : "fa",
  );
  const [destSlug, setDestSlug] = useState("");
  const [resolvedDestination, setResolvedDestination] = useState<{
    id: string;
    code: string;
    englishName: string;
    kind: string;
    slug: string;
  } | null>(null);

  function mapAuthError(status?: number) {
    if (status === 401 || status === 403) return copy.unauthorizedBody;
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

  function applyDetail(next: ContentDetailView) {
    setDetail(next);
    const row = next.translations.find(
      (t) => t.localeCode.toLowerCase() === translationLocale,
    );
    setTitle(row?.title ?? "");
    setBody(row?.body ?? "");
    setExcerpt(row?.excerpt ?? "");
    setContentSlug(row?.slug ?? "");
    setResolvedDestination(null);
    setDestSlug("");
  }

  async function refreshReadyMedia() {
    const result = await listMediaAssetsAction({ status: "Ready", take: 48 });
    if (!result.ok) {
      setError(mapAuthError(result.status));
      return;
    }
    setReadyMedia(result.items);
    setReadyMediaLoaded(true);
  }

  function mediaPreviewSrc(assetId: string): string {
    return resolveMediaAppProxySrc(
      mediaVariantContentPath(assetId, "thumbnail"),
    );
  }

  async function refreshList() {
    const result = await listContentItemsAction({
      kind: kindFilter || undefined,
      take,
    });
    if (!result.ok) {
      setError(mapAuthError(result.status));
      return;
    }
    setItems(result.items);
  }

  async function refreshTaxonomy() {
    const [cats, tagList] = await Promise.all([
      listContentCategoriesAction({ take: 100 }),
      listContentTagsAction({ take: 100 }),
    ]);
    if (!cats.ok) {
      setError(mapAuthError(cats.status));
      return;
    }
    if (!tagList.ok) {
      setError(mapAuthError(tagList.status));
      return;
    }
    setCategories(cats.items);
    setTags(tagList.items);
    if (!selectedCategoryId && cats.items[0]) {
      setSelectedCategoryId(cats.items[0].id);
    }
    if (!selectedTagId && tagList.items[0]) {
      setSelectedTagId(tagList.items[0].id);
    }
  }

  async function openItem(id: string) {
    const result = await loadContentDetailAction(id);
    if (!result.ok) {
      setError(mapAuthError(result.status));
      setDetail(null);
      return;
    }
    applyDetail(result.detail);
  }

  async function reloadSelected() {
    if (!detail) return;
    await openItem(detail.item.id);
  }

  if (!apiConfigured) {
    return (
      <Surface tone="muted">
        <Text role="muted">{copy.apiMissing}</Text>
      </Surface>
    );
  }

  const selected = detail?.item ?? null;
  const blocks = detail?.blocks ?? [];

  return (
    <Stack gap="lg">
      {error ? (
        <div role="alert">
          <Surface tone="muted">
            <Text role="muted">{error}</Text>
          </Surface>
        </div>
      ) : null}
      {pending ? <Text role="caption">{copy.pending}</Text> : null}

      <Surface>
        <Stack gap="md">
          <Text as="h2" role="heading">
            {copy.stepCreate}
          </Text>
          <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-kind`}>
            {copy.kindLabel}
            <select
              id={`${formId}-kind`}
              className="min-h-touch rounded-md border border-border bg-background px-3"
              value={createKind}
              onChange={(e) =>
                setCreateKind(e.target.value as "Article" | "LandingPage" | "Guide")
              }
            >
              <option value="Article">Article</option>
              <option value="LandingPage">LandingPage</option>
              <option value="Guide">Guide</option>
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-code`}>
            {copy.codeLabel}
            <LtrValue>
              <input
                id={`${formId}-code`}
                className="min-h-touch w-full rounded-md border border-border bg-background px-3"
                value={createCode}
                onChange={(e) => setCreateCode(e.target.value)}
              />
            </LtrValue>
          </label>
          <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-name`}>
            {copy.englishNameLabel}
            <input
              id={`${formId}-name`}
              className="min-h-touch rounded-md border border-border bg-background px-3"
              value={createName}
              onChange={(e) => setCreateName(e.target.value)}
            />
          </label>
          <button
            type="button"
            className="min-h-touch rounded-md bg-foreground px-4 text-background"
            disabled={pending}
            onClick={() =>
              run(async () => {
                const result = await createContentItemAction({
                  kind: createKind,
                  code: createCode,
                  englishName: createName,
                });
                if (!result.ok) {
                  setError(mapAuthError(result.status) === copy.unauthorizedBody
                    ? copy.unauthorizedBody
                    : result.message || copy.errorGeneric);
                  return;
                }
                setCreateCode("");
                setCreateName("");
                await openItem(result.item.id);
                await refreshList();
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
          <div className="flex flex-wrap gap-3">
            <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-filter`}>
              {copy.kindFilterLabel}
              <select
                id={`${formId}-filter`}
                className="min-h-touch rounded-md border border-border bg-background px-3"
                value={kindFilter}
                onChange={(e) => setKindFilter(e.target.value)}
              >
                <option value="">{copy.kindAll}</option>
                <option value="Article">Article</option>
                <option value="LandingPage">LandingPage</option>
                <option value="Guide">Guide</option>
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-take`}>
              {copy.takeLabel}
              <input
                id={`${formId}-take`}
                type="number"
                min={1}
                max={200}
                className="min-h-touch w-24 rounded-md border border-border bg-background px-3"
                value={take}
                onChange={(e) => setTake(Number(e.target.value) || 50)}
              />
            </label>
            <button
              type="button"
              className="min-h-touch self-end rounded-md border border-border px-4"
              disabled={pending}
              onClick={() => run(refreshList)}
            >
              {copy.refreshList}
            </button>
          </div>
          {items.length === 0 ? (
            <Text role="muted">{copy.noItems}</Text>
          ) : (
            <ul className="flex flex-col gap-2">
              {items.map((item) => (
                <li key={item.id} className="flex flex-wrap items-center gap-2 text-sm">
                  <LtrValue>
                    <span>
                      {item.kind} · {item.code} · {item.englishName}
                    </span>
                  </LtrValue>
                  <button
                    type="button"
                    className="min-h-touch rounded-md border border-border px-3"
                    disabled={pending}
                    onClick={() => run(async () => openItem(item.id))}
                  >
                    {copy.selectItem}
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
          <Text role="muted">{copy.openByCodeHint}</Text>
          <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-open`}>
            {copy.codeLabel}
            <LtrValue>
              <input
                id={`${formId}-open`}
                className="min-h-touch w-full rounded-md border border-border bg-background px-3"
                value={openCode}
                onChange={(e) => setOpenCode(e.target.value)}
              />
            </LtrValue>
          </label>
          <button
            type="button"
            className="min-h-touch rounded-md border border-border px-4"
            disabled={pending}
            onClick={() =>
              run(async () => {
                const result = await openContentByCodeAction({
                  code: openCode,
                  locale: translationLocale,
                });
                if (!result.ok) {
                  setError(mapAuthError(result.status));
                  return;
                }
                applyDetail(result.detail);
              })
            }
          >
            {copy.openByCodeAction}
          </button>
        </Stack>
      </Surface>

      {selected ? (
        <>
          <Surface>
            <Stack gap="md">
              <Text as="h2" role="heading">
                {copy.selectedTitle}
              </Text>
              <Text role="muted">{copy.metadataHeading}</Text>
              <LtrValue>
                <Text>
                  {selected.kind} · {selected.code} · {selected.englishName}
                </Text>
              </LtrValue>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="md">
              <Text as="h2" role="heading">
                {copy.stepTranslate}
              </Text>
              <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-tloc`}>
                {copy.translationLocale}
                <select
                  id={`${formId}-tloc`}
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                  value={translationLocale}
                  onChange={(e) => {
                    const next = e.target.value as "fa" | "en";
                    setTranslationLocale(next);
                    const row = detail?.translations.find(
                      (t) => t.localeCode.toLowerCase() === next,
                    );
                    setTitle(row?.title ?? "");
                    setBody(row?.body ?? "");
                    setExcerpt(row?.excerpt ?? "");
                    setContentSlug(row?.slug ?? "");
                  }}
                >
                  <option value="fa">fa</option>
                  <option value="en">en</option>
                </select>
              </label>
              <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-title`}>
                {copy.titleLabel}
                <input
                  id={`${formId}-title`}
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                />
              </label>
              <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-body`}>
                {copy.bodyLabel}
                <textarea
                  id={`${formId}-body`}
                  className="min-h-32 rounded-md border border-border bg-background px-3 py-2"
                  value={body}
                  onChange={(e) => setBody(e.target.value)}
                />
              </label>
              <label className="flex flex-col gap-1 text-sm" htmlFor={`${formId}-excerpt`}>
                {copy.excerptLabel}
                <input
                  id={`${formId}-excerpt`}
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                  value={excerpt}
                  onChange={(e) => setExcerpt(e.target.value)}
                />
              </label>
              <button
                type="button"
                className="min-h-touch rounded-md bg-foreground px-4 text-background"
                disabled={pending}
                onClick={() =>
                  run(async () => {
                    const result = await upsertContentTranslationAction({
                      contentItemId: selected.id,
                      localeCode: translationLocale,
                      title,
                      body: body || null,
                      excerpt: excerpt || null,
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status));
                      return;
                    }
                    await reloadSelected();
                  })
                }
              >
                {copy.saveTranslation}
              </button>
              <form
                className="grid gap-3 sm:grid-cols-2"
                onSubmit={(e) => {
                  e.preventDefault();
                  run(async () => {
                    const result = await setContentTranslationSlugAction({
                      contentItemId: selected.id,
                      localeCode: translationLocale,
                      slug: contentSlug.trim() || null,
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status));
                      return;
                    }
                    await reloadSelected();
                  });
                }}
              >
                <label className="flex flex-col gap-1 text-sm sm:col-span-2">
                  <span>{copy.slugLabel}</span>
                  <input
                    value={contentSlug}
                    onChange={(e) => setContentSlug(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                  <Text role="caption">{copy.slugHint}</Text>
                </label>
                <div className="flex flex-wrap gap-2 sm:col-span-2">
                  <button
                    type="submit"
                    disabled={pending}
                    className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                  >
                    {copy.saveSlug}
                  </button>
                  <button
                    type="button"
                    disabled={
                      pending ||
                      !contentSlug.trim() ||
                      (selected.kind !== "Article" &&
                        selected.kind !== "LandingPage")
                    }
                    className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                    onClick={() =>
                      run(async () => {
                        const result = await publishContentSeoRouteAction({
                          contentItemId: selected.id,
                          localeCode: translationLocale,
                          slug: contentSlug.trim(),
                          kind: selected.kind,
                        });
                        if (!result.ok) {
                          setError(mapAuthError(result.status));
                          return;
                        }
                      })
                    }
                  >
                    {copy.publishSeoRoute}
                  </button>
                </div>
                <Text role="caption">{copy.publishSeoHint}</Text>
              </form>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="md">
              <Text as="h2" role="heading">
                {copy.stepTaxonomy}
              </Text>
              <button
                type="button"
                className="min-h-touch w-fit rounded-md border border-border px-4"
                disabled={pending}
                onClick={() => run(refreshTaxonomy)}
              >
                {copy.refreshList}
              </button>
              <div className="grid gap-4 md:grid-cols-2">
                <Stack gap="sm">
                  <Text role="heading">{copy.categoriesHeading}</Text>
                  <label className="flex flex-col gap-1 text-sm">
                    {copy.categoryCodeLabel}
                    <LtrValue>
                      <input
                        className="min-h-touch w-full rounded-md border border-border bg-background px-3"
                        value={categoryCode}
                        onChange={(e) => setCategoryCode(e.target.value)}
                      />
                    </LtrValue>
                  </label>
                  <label className="flex flex-col gap-1 text-sm">
                    {copy.englishNameLabel}
                    <input
                      className="min-h-touch rounded-md border border-border bg-background px-3"
                      value={categoryName}
                      onChange={(e) => setCategoryName(e.target.value)}
                    />
                  </label>
                  <button
                    type="button"
                    className="min-h-touch rounded-md border border-border px-4"
                    disabled={pending}
                    onClick={() =>
                      run(async () => {
                        const created = await createContentCategoryAction({
                          code: categoryCode,
                          englishName: categoryName || categoryCode,
                        });
                        if (!created.ok) {
                          setError(mapAuthError(created.status));
                          return;
                        }
                        setCategoryCode("");
                        setCategoryName("");
                        await refreshTaxonomy();
                      })
                    }
                  >
                    {copy.createCategory}
                  </button>
                  <select
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                    value={selectedCategoryId}
                    onChange={(e) => setSelectedCategoryId(e.target.value)}
                  >
                    {categories.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.code} · {c.englishName}
                      </option>
                    ))}
                  </select>
                  <div className="flex flex-wrap gap-2">
                    <button
                      type="button"
                      className="min-h-touch rounded-md border border-border px-3"
                      disabled={pending || !selectedCategoryId}
                      onClick={() =>
                        run(async () => {
                          const result = await assignContentCategoryAction({
                            contentItemId: selected.id,
                            categoryId: selectedCategoryId,
                          });
                          if (!result.ok) {
                            setError(mapAuthError(result.status));
                            return;
                          }
                          await reloadSelected();
                        })
                      }
                    >
                      {copy.assignCategory}
                    </button>
                    <button
                      type="button"
                      className="min-h-touch rounded-md border border-border px-3"
                      disabled={pending || !selectedCategoryId}
                      onClick={() =>
                        run(async () => {
                          const result = await removeContentCategoryAction({
                            contentItemId: selected.id,
                            categoryId: selectedCategoryId,
                          });
                          if (!result.ok) {
                            setError(mapAuthError(result.status));
                            return;
                          }
                          await reloadSelected();
                        })
                      }
                    >
                      {copy.removeCategory}
                    </button>
                  </div>
                  <Text role="caption">
                    <LtrValue>
                      {(selected.categoryIds ?? []).join(", ") || "—"}
                    </LtrValue>
                  </Text>
                </Stack>
                <Stack gap="sm">
                  <Text role="heading">{copy.tagsHeading}</Text>
                  <label className="flex flex-col gap-1 text-sm">
                    {copy.tagCodeLabel}
                    <LtrValue>
                      <input
                        className="min-h-touch w-full rounded-md border border-border bg-background px-3"
                        value={tagCode}
                        onChange={(e) => setTagCode(e.target.value)}
                      />
                    </LtrValue>
                  </label>
                  <label className="flex flex-col gap-1 text-sm">
                    {copy.englishNameLabel}
                    <input
                      className="min-h-touch rounded-md border border-border bg-background px-3"
                      value={tagName}
                      onChange={(e) => setTagName(e.target.value)}
                    />
                  </label>
                  <button
                    type="button"
                    className="min-h-touch rounded-md border border-border px-4"
                    disabled={pending}
                    onClick={() =>
                      run(async () => {
                        const created = await createContentTagAction({
                          code: tagCode,
                          englishName: tagName || tagCode,
                        });
                        if (!created.ok) {
                          setError(mapAuthError(created.status));
                          return;
                        }
                        setTagCode("");
                        setTagName("");
                        await refreshTaxonomy();
                      })
                    }
                  >
                    {copy.createTag}
                  </button>
                  <select
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                    value={selectedTagId}
                    onChange={(e) => setSelectedTagId(e.target.value)}
                  >
                    {tags.map((t) => (
                      <option key={t.id} value={t.id}>
                        {t.code} · {t.englishName}
                      </option>
                    ))}
                  </select>
                  <div className="flex flex-wrap gap-2">
                    <button
                      type="button"
                      className="min-h-touch rounded-md border border-border px-3"
                      disabled={pending || !selectedTagId}
                      onClick={() =>
                        run(async () => {
                          const result = await assignContentTagAction({
                            contentItemId: selected.id,
                            tagId: selectedTagId,
                          });
                          if (!result.ok) {
                            setError(mapAuthError(result.status));
                            return;
                          }
                          await reloadSelected();
                        })
                      }
                    >
                      {copy.assignTag}
                    </button>
                    <button
                      type="button"
                      className="min-h-touch rounded-md border border-border px-3"
                      disabled={pending || !selectedTagId}
                      onClick={() =>
                        run(async () => {
                          const result = await removeContentTagAction({
                            contentItemId: selected.id,
                            tagId: selectedTagId,
                          });
                          if (!result.ok) {
                            setError(mapAuthError(result.status));
                            return;
                          }
                          await reloadSelected();
                        })
                      }
                    >
                      {copy.removeTag}
                    </button>
                  </div>
                  <Text role="caption">
                    <LtrValue>{(selected.tagIds ?? []).join(", ") || "—"}</LtrValue>
                  </Text>
                </Stack>
              </div>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="md">
              <Text as="h2" role="heading">
                {copy.stepBlocks}
              </Text>
              <Text role="muted">{copy.blocksHeading}</Text>
              <label className="flex flex-col gap-1 text-sm">
                {copy.headingTextLabel}
                <input
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                  value={headingText}
                  onChange={(e) => setHeadingText(e.target.value)}
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                {copy.headingLevelLabel}
                <input
                  type="number"
                  min={1}
                  max={6}
                  className="min-h-touch w-24 rounded-md border border-border bg-background px-3"
                  value={headingLevel}
                  onChange={(e) => setHeadingLevel(e.target.value)}
                />
              </label>
              <button
                type="button"
                className="min-h-touch w-fit rounded-md border border-border px-4"
                disabled={pending}
                onClick={() =>
                  run(async () => {
                    const result = await addContentHeadingBlockAction({
                      contentItemId: selected.id,
                      text: headingText,
                      level: Number(headingLevel) || 2,
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status));
                      return;
                    }
                    setHeadingText("");
                    await reloadSelected();
                  })
                }
              >
                {copy.addHeading}
              </button>
              <label className="flex flex-col gap-1 text-sm">
                {copy.paragraphTextLabel}
                <textarea
                  className="min-h-24 rounded-md border border-border bg-background px-3 py-2"
                  value={paragraphText}
                  onChange={(e) => setParagraphText(e.target.value)}
                />
              </label>
              <button
                type="button"
                className="min-h-touch w-fit rounded-md border border-border px-4"
                disabled={pending}
                onClick={() =>
                  run(async () => {
                    const result = await addContentParagraphBlockAction({
                      contentItemId: selected.id,
                      text: paragraphText,
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status));
                      return;
                    }
                    setParagraphText("");
                    await reloadSelected();
                  })
                }
              >
                {copy.addParagraph}
              </button>

              <Text role="heading">{copy.mediaPickerHeading}</Text>
              <Text role="muted">{copy.mediaPickerHint}</Text>
              <button
                type="button"
                className="min-h-touch w-fit rounded-md border border-border px-4"
                disabled={pending}
                onClick={() => run(refreshReadyMedia)}
              >
                {copy.refreshReadyMedia}
              </button>
              {readyMediaLoaded && readyMedia.length === 0 ? (
                <Text role="muted">{copy.noReadyMedia}</Text>
              ) : null}
              <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                {readyMedia.map((asset) => (
                  <li key={asset.id} className="rounded-md border border-border p-2">
                    {/* eslint-disable-next-line @next/next/no-img-element */}
                    <img
                      alt={asset.contentType}
                      className="mb-2 h-24 w-full object-cover"
                      src={mediaPreviewSrc(asset.id)}
                    />
                    <LtrValue>
                      <Text role="caption">{asset.id}</Text>
                    </LtrValue>
                    <button
                      type="button"
                      className="mt-2 min-h-touch w-full rounded-md border border-border px-2 text-sm"
                      disabled={pending}
                      onClick={() =>
                        run(async () => {
                          const result = await addContentImageBlockAction({
                            contentItemId: selected.id,
                            mediaAssetId: asset.id,
                          });
                          if (!result.ok) {
                            setError(mapAuthError(result.status));
                            return;
                          }
                          await reloadSelected();
                        })
                      }
                    >
                      {copy.addImageBlock}
                    </button>
                  </li>
                ))}
              </ul>

              <ul className="flex flex-col gap-2">
                {blocks.map((block, index) => (
                  <li
                    key={block.id}
                    className="flex flex-wrap items-center gap-2 rounded-md border border-border p-2 text-sm"
                  >
                    <LtrValue>
                      <span>
                        #{block.sortOrder} · {block.kind}
                        {block.text ? ` · ${block.text.slice(0, 48)}` : ""}
                        {block.mediaAssetId ? ` · media` : ""}
                      </span>
                    </LtrValue>
                    <button
                      type="button"
                      className="min-h-touch rounded-md border border-border px-2"
                      disabled={pending || index === 0}
                      onClick={() =>
                        run(async () => {
                          const ids = blocks.map((b) => b.id);
                          const next = [...ids];
                          const tmp = next[index - 1]!;
                          next[index - 1] = next[index]!;
                          next[index] = tmp;
                          const result = await reorderContentBlocksAction({
                            contentItemId: selected.id,
                            orderedBlockIds: next,
                          });
                          if (!result.ok) {
                            setError(mapAuthError(result.status));
                            return;
                          }
                          await reloadSelected();
                        })
                      }
                    >
                      {copy.reorderUp}
                    </button>
                    <button
                      type="button"
                      className="min-h-touch rounded-md border border-border px-2"
                      disabled={pending || index >= blocks.length - 1}
                      onClick={() =>
                        run(async () => {
                          const ids = blocks.map((b) => b.id);
                          const next = [...ids];
                          const tmp = next[index + 1]!;
                          next[index + 1] = next[index]!;
                          next[index] = tmp;
                          const result = await reorderContentBlocksAction({
                            contentItemId: selected.id,
                            orderedBlockIds: next,
                          });
                          if (!result.ok) {
                            setError(mapAuthError(result.status));
                            return;
                          }
                          await reloadSelected();
                        })
                      }
                    >
                      {copy.reorderDown}
                    </button>
                    <button
                      type="button"
                      className="min-h-touch rounded-md border border-border px-2"
                      disabled={pending}
                      onClick={() =>
                        run(async () => {
                          const result = await removeContentBlockAction({
                            contentItemId: selected.id,
                            blockId: block.id,
                          });
                          if (!result.ok) {
                            setError(mapAuthError(result.status));
                            return;
                          }
                          await reloadSelected();
                        })
                      }
                    >
                      {copy.removeBlock}
                    </button>
                  </li>
                ))}
              </ul>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="md">
              <Text as="h2" role="heading">
                {copy.stepDestination}
              </Text>
              <label className="flex flex-col gap-1 text-sm">
                {copy.destinationSlugLocale}
                <select
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                  value={destSlugLocale}
                  onChange={(e) => setDestSlugLocale(e.target.value as "fa" | "en")}
                >
                  <option value="fa">fa</option>
                  <option value="en">en</option>
                </select>
              </label>
              <label className="flex flex-col gap-1 text-sm">
                {copy.destinationSlugLabel}
                <LtrValue>
                  <input
                    className="min-h-touch w-full rounded-md border border-border bg-background px-3"
                    value={destSlug}
                    onChange={(e) => setDestSlug(e.target.value)}
                  />
                </LtrValue>
              </label>
              <button
                type="button"
                className="min-h-touch w-fit rounded-md border border-border px-4"
                disabled={pending}
                onClick={() =>
                  run(async () => {
                    const result = await resolveDestinationBySlugAction({
                      localeCode: destSlugLocale,
                      slug: destSlug,
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status));
                      setResolvedDestination(null);
                      return;
                    }
                    setResolvedDestination(result.destination);
                  })
                }
              >
                {copy.resolveDestination}
              </button>
              {resolvedDestination ? (
                <Text role="muted">
                  {copy.destinationResolved}:{" "}
                  <LtrValue>
                    {resolvedDestination.code} · {resolvedDestination.englishName}
                  </LtrValue>
                </Text>
              ) : null}
              <div className="flex flex-wrap gap-2">
                <button
                  type="button"
                  className="min-h-touch rounded-md bg-foreground px-4 text-background"
                  disabled={pending || !resolvedDestination}
                  onClick={() =>
                    run(async () => {
                      if (!resolvedDestination) return;
                      const result = await assignContentDestinationAction({
                        contentItemId: selected.id,
                        destinationId: resolvedDestination.id,
                      });
                      if (!result.ok) {
                        setError(mapAuthError(result.status));
                        return;
                      }
                      await reloadSelected();
                    })
                  }
                >
                  {copy.saveDestinationLink}
                </button>
                <button
                  type="button"
                  className="min-h-touch rounded-md border border-border px-4"
                  disabled={pending || !resolvedDestination}
                  onClick={() =>
                    run(async () => {
                      if (!resolvedDestination) return;
                      const result = await removeContentDestinationAction({
                        contentItemId: selected.id,
                        destinationId: resolvedDestination.id,
                      });
                      if (!result.ok) {
                        setError(mapAuthError(result.status));
                        return;
                      }
                      await reloadSelected();
                    })
                  }
                >
                  {copy.removeDestination}
                </button>
              </div>
              <Text role="caption">
                <LtrValue>
                  {(selected.destinationIds ?? []).join(", ") || "—"}
                </LtrValue>
              </Text>
            </Stack>
          </Surface>
        </>
      ) : (
        <Surface tone="muted">
          <Text role="muted">{copy.stepInspect}</Text>
        </Surface>
      )}
    </Stack>
  );
}
