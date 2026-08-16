"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import type {
  ContentBlockView,
  ContentCategoryView,
  ContentDetailView,
  ContentItemSummaryView,
  ContentTagView,
  ContentTranslationView,
} from "@/features/admin-content/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiContentItem = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  localizedTitle?: string | null;
  localizedBody?: string | null;
  localizedExcerpt?: string | null;
  categoryIds?: string[] | null;
  tagIds?: string[] | null;
  destinationIds?: string[] | null;
  createdAt: string;
  updatedAt: string;
};

type ApiTranslation = {
  contentItemId: string;
  localeCode: string;
  title: string;
  body?: string | null;
  excerpt?: string | null;
  updatedAt: string;
};

type ApiCategory = {
  id: string;
  code: string;
  englishName: string;
  createdAt: string;
  updatedAt: string;
};

type ApiTag = {
  id: string;
  code: string;
  englishName: string;
  createdAt: string;
  updatedAt: string;
};

type ApiBlock = {
  id: string;
  contentItemId: string;
  kind: string;
  sortOrder: number;
  text?: string | null;
  headingLevel?: number | null;
  mediaAssetId?: string | null;
  href?: string | null;
  galleryItems?: { mediaAssetId: string; sortOrder: number }[] | null;
  faqItems?: { question: string; answer: string; sortOrder: number }[] | null;
};

async function authHeaders(): Promise<HeadersInit> {
  const jar = await cookies();
  const ticket = jar.get(AUTH_COOKIE)?.value;
  const headers = new Headers();
  if (ticket) {
    headers.set("cookie", `${AUTH_COOKIE}=${ticket}`);
  }
  return headers;
}

function mapItem(p: ApiContentItem): ContentItemSummaryView {
  return {
    id: p.id,
    kind: p.kind,
    code: p.code,
    englishName: p.englishName,
    localizedTitle: p.localizedTitle ?? null,
    localizedBody: p.localizedBody ?? null,
    localizedExcerpt: p.localizedExcerpt ?? null,
    categoryIds: p.categoryIds ?? [],
    tagIds: p.tagIds ?? [],
    destinationIds: p.destinationIds ?? [],
    createdAt: p.createdAt,
    updatedAt: p.updatedAt,
  };
}

function mapTranslation(t: ApiTranslation): ContentTranslationView {
  return {
    contentItemId: t.contentItemId,
    localeCode: t.localeCode,
    title: t.title,
    body: t.body ?? null,
    excerpt: t.excerpt ?? null,
    updatedAt: t.updatedAt,
  };
}

function mapCategory(c: ApiCategory): ContentCategoryView {
  return {
    id: c.id,
    code: c.code,
    englishName: c.englishName,
    createdAt: c.createdAt,
    updatedAt: c.updatedAt,
  };
}

function mapTag(t: ApiTag): ContentTagView {
  return {
    id: t.id,
    code: t.code,
    englishName: t.englishName,
    createdAt: t.createdAt,
    updatedAt: t.updatedAt,
  };
}

function mapBlock(b: ApiBlock): ContentBlockView {
  return {
    id: b.id,
    contentItemId: b.contentItemId,
    kind: b.kind,
    sortOrder: b.sortOrder,
    text: b.text ?? null,
    headingLevel: b.headingLevel ?? null,
    mediaAssetId: b.mediaAssetId ?? null,
    href: b.href ?? null,
    galleryItems: (b.galleryItems ?? []).map((g) => ({
      mediaAssetId: g.mediaAssetId,
      sortOrder: g.sortOrder,
    })),
    faqItems: (b.faqItems ?? []).map((f) => ({
      question: f.question,
      answer: f.answer,
      sortOrder: f.sortOrder,
    })),
  };
}

function failMessage(
  result: { message: string; status?: number },
): { ok: false; message: string; status?: number } {
  return { ok: false, message: result.message, status: result.status };
}

export async function listContentItemsAction(input: {
  kind?: string;
  take?: number;
}): Promise<
  | { ok: true; items: ContentItemSummaryView[] }
  | { ok: false; message: string; status?: number }
> {
  const take = Math.min(Math.max(input.take ?? 50, 1), 200);
  const params = new URLSearchParams();
  params.set("take", String(take));
  if (input.kind && input.kind.trim()) {
    params.set("kind", input.kind.trim());
  }
  const result = await apiGetJson<ApiContentItem[]>(
    `/api/content/items/?${params.toString()}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, items: (result.data ?? []).map(mapItem) };
}

export async function createContentItemAction(input: {
  kind: string;
  code: string;
  englishName: string;
}): Promise<
  | { ok: true; item: ContentItemSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiContentItem>("/api/content/items/", {
    method: "POST",
    headers: await authHeaders(),
    body: {
      kind: input.kind,
      code: input.code,
      englishName: input.englishName,
    },
  });
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapItem(result.data) };
}

export async function openContentByCodeAction(input: {
  code: string;
  locale?: string;
}): Promise<
  | { ok: true; detail: ContentDetailView }
  | { ok: false; message: string; status?: number }
> {
  const code = encodeURIComponent(input.code.trim());
  const params = new URLSearchParams();
  if (input.locale) params.set("locale", input.locale);
  const qs = params.toString();
  const headers = await authHeaders();
  const itemResult = await apiGetJson<ApiContentItem>(
    `/api/content/items/by-code/${code}${qs ? `?${qs}` : ""}`,
    { headers, cache: "no-store" },
  );
  if (!itemResult.ok) return failMessage(itemResult);
  return loadContentDetailBundle(itemResult.data.id, headers);
}

export async function loadContentDetailAction(
  contentItemId: string,
): Promise<
  | { ok: true; detail: ContentDetailView }
  | { ok: false; message: string; status?: number }
> {
  return loadContentDetailBundle(contentItemId, await authHeaders());
}

async function loadContentDetailBundle(
  contentItemId: string,
  headers: HeadersInit,
): Promise<
  | { ok: true; detail: ContentDetailView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(contentItemId.trim());
  const itemResult = await apiGetJson<ApiContentItem>(
    `/api/content/items/${id}`,
    { headers, cache: "no-store" },
  );
  if (!itemResult.ok) return failMessage(itemResult);

  const translationsResult = await apiGetJson<ApiTranslation[]>(
    `/api/content/items/${id}/translations`,
    { headers, cache: "no-store" },
  );
  if (!translationsResult.ok) return failMessage(translationsResult);

  const blocksResult = await apiGetJson<ApiBlock[]>(
    `/api/content/items/${id}/blocks`,
    { headers, cache: "no-store" },
  );
  if (!blocksResult.ok) return failMessage(blocksResult);

  return {
    ok: true,
    detail: {
      item: mapItem(itemResult.data),
      translations: (translationsResult.data ?? []).map(mapTranslation),
      blocks: (blocksResult.data ?? []).map(mapBlock),
    },
  };
}

export async function upsertContentTranslationAction(input: {
  contentItemId: string;
  localeCode: string;
  title: string;
  body?: string | null;
  excerpt?: string | null;
}): Promise<
  | { ok: true; translation: ContentTranslationView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const locale = encodeURIComponent(input.localeCode.trim());
  const result = await apiSendJson<ApiTranslation>(
    `/api/content/items/${id}/translations/${locale}`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: {
        title: input.title,
        body: input.body ?? null,
        excerpt: input.excerpt ?? null,
      },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, translation: mapTranslation(result.data) };
}

export async function listContentCategoriesAction(input?: {
  take?: number;
}): Promise<
  | { ok: true; items: ContentCategoryView[] }
  | { ok: false; message: string; status?: number }
> {
  const take = Math.min(Math.max(input?.take ?? 100, 1), 200);
  const result = await apiGetJson<ApiCategory[]>(
    `/api/content/categories/?take=${take}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, items: (result.data ?? []).map(mapCategory) };
}

export async function createContentCategoryAction(input: {
  code: string;
  englishName: string;
}): Promise<
  | { ok: true; category: ContentCategoryView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiCategory>("/api/content/categories/", {
    method: "POST",
    headers: await authHeaders(),
    body: { code: input.code, englishName: input.englishName },
  });
  if (!result.ok) return failMessage(result);
  return { ok: true, category: mapCategory(result.data) };
}

export async function listContentTagsAction(input?: {
  take?: number;
}): Promise<
  | { ok: true; items: ContentTagView[] }
  | { ok: false; message: string; status?: number }
> {
  const take = Math.min(Math.max(input?.take ?? 100, 1), 200);
  const result = await apiGetJson<ApiTag[]>(
    `/api/content/tags/?take=${take}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, items: (result.data ?? []).map(mapTag) };
}

export async function createContentTagAction(input: {
  code: string;
  englishName: string;
}): Promise<
  | { ok: true; tag: ContentTagView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiTag>("/api/content/tags/", {
    method: "POST",
    headers: await authHeaders(),
    body: { code: input.code, englishName: input.englishName },
  });
  if (!result.ok) return failMessage(result);
  return { ok: true, tag: mapTag(result.data) };
}

export async function assignContentCategoryAction(input: {
  contentItemId: string;
  categoryId: string;
}): Promise<
  | { ok: true; item: ContentItemSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const categoryId = encodeURIComponent(input.categoryId.trim());
  const result = await apiSendJson<ApiContentItem>(
    `/api/content/items/${id}/categories/${categoryId}`,
    { method: "POST", headers: await authHeaders() },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapItem(result.data) };
}

export async function removeContentCategoryAction(input: {
  contentItemId: string;
  categoryId: string;
}): Promise<
  | { ok: true; item: ContentItemSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const categoryId = encodeURIComponent(input.categoryId.trim());
  const result = await apiSendJson<ApiContentItem>(
    `/api/content/items/${id}/categories/${categoryId}`,
    { method: "DELETE", headers: await authHeaders() },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapItem(result.data) };
}

export async function assignContentTagAction(input: {
  contentItemId: string;
  tagId: string;
}): Promise<
  | { ok: true; item: ContentItemSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const tagId = encodeURIComponent(input.tagId.trim());
  const result = await apiSendJson<ApiContentItem>(
    `/api/content/items/${id}/tags/${tagId}`,
    { method: "POST", headers: await authHeaders() },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapItem(result.data) };
}

export async function removeContentTagAction(input: {
  contentItemId: string;
  tagId: string;
}): Promise<
  | { ok: true; item: ContentItemSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const tagId = encodeURIComponent(input.tagId.trim());
  const result = await apiSendJson<ApiContentItem>(
    `/api/content/items/${id}/tags/${tagId}`,
    { method: "DELETE", headers: await authHeaders() },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapItem(result.data) };
}

export async function assignContentDestinationAction(input: {
  contentItemId: string;
  destinationId: string;
}): Promise<
  | { ok: true; item: ContentItemSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const destinationId = encodeURIComponent(input.destinationId.trim());
  const result = await apiSendJson<ApiContentItem>(
    `/api/content/items/${id}/destinations/${destinationId}`,
    { method: "POST", headers: await authHeaders() },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapItem(result.data) };
}

export async function removeContentDestinationAction(input: {
  contentItemId: string;
  destinationId: string;
}): Promise<
  | { ok: true; item: ContentItemSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const destinationId = encodeURIComponent(input.destinationId.trim());
  const result = await apiSendJson<ApiContentItem>(
    `/api/content/items/${id}/destinations/${destinationId}`,
    { method: "DELETE", headers: await authHeaders() },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapItem(result.data) };
}

export async function addContentHeadingBlockAction(input: {
  contentItemId: string;
  text: string;
  level: number;
}): Promise<
  | { ok: true; block: ContentBlockView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const result = await apiSendJson<ApiBlock>(
    `/api/content/items/${id}/blocks/heading`,
    {
      method: "POST",
      headers: await authHeaders(),
      body: { text: input.text, level: input.level },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, block: mapBlock(result.data) };
}

export async function addContentParagraphBlockAction(input: {
  contentItemId: string;
  text: string;
}): Promise<
  | { ok: true; block: ContentBlockView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const result = await apiSendJson<ApiBlock>(
    `/api/content/items/${id}/blocks/paragraph`,
    {
      method: "POST",
      headers: await authHeaders(),
      body: { text: input.text },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, block: mapBlock(result.data) };
}

export async function addContentImageBlockAction(input: {
  contentItemId: string;
  mediaAssetId: string;
  caption?: string | null;
}): Promise<
  | { ok: true; block: ContentBlockView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const result = await apiSendJson<ApiBlock>(
    `/api/content/items/${id}/blocks/image`,
    {
      method: "POST",
      headers: await authHeaders(),
      body: {
        mediaAssetId: input.mediaAssetId,
        caption: input.caption ?? null,
      },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, block: mapBlock(result.data) };
}

export async function removeContentBlockAction(input: {
  contentItemId: string;
  blockId: string;
}): Promise<{ ok: true } | { ok: false; message: string; status?: number }> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const blockId = encodeURIComponent(input.blockId.trim());
  const result = await apiSendJson<unknown>(
    `/api/content/items/${id}/blocks/${blockId}`,
    { method: "DELETE", headers: await authHeaders() },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true };
}

export async function reorderContentBlocksAction(input: {
  contentItemId: string;
  orderedBlockIds: string[];
}): Promise<
  | { ok: true; blocks: ContentBlockView[] }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.contentItemId.trim());
  const result = await apiSendJson<ApiBlock[]>(
    `/api/content/items/${id}/blocks/reorder`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: { orderedBlockIds: input.orderedBlockIds },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, blocks: (result.data ?? []).map(mapBlock) };
}
