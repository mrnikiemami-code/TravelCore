"use client";

import { useEffect, useId, useMemo, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import {
  createDestinationAction,
  listCountriesAction,
  loadDestinationBundleAction,
  openBySlugAction,
  setGeoAction,
  setTranslationSlugAction,
  upsertTranslationAction,
} from "@/features/admin-destination-hierarchy/actions";
import { getDestinationHierarchyWorkflowCopy } from "@/features/admin-destination-hierarchy/copy";
import type {
  CountryCatalogView,
  DestinationPathNodeView,
  DestinationPathView,
  DestinationSummaryView,
  DestinationTranslationView,
} from "@/features/admin-destination-hierarchy/types";

export type DestinationHierarchyWorkflowIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

function childKindsFor(parentKind: string | null): string[] {
  if (!parentKind) return ["Country"];
  switch (parentKind) {
    case "Country":
      return ["Region", "City"];
    case "Region":
      return ["City", "Area"];
    case "City":
      return ["Area"];
    default:
      return [];
  }
}

function kindLabel(
  kind: string,
  copy: ReturnType<typeof getDestinationHierarchyWorkflowCopy>,
): string {
  switch (kind) {
    case "Country":
      return copy.kindCountry;
    case "Region":
      return copy.kindRegion;
    case "City":
      return copy.kindCity;
    case "Area":
      return copy.kindArea;
    default:
      return kind;
  }
}

export function DestinationHierarchyWorkflowIsland({
  locale,
  apiConfigured,
}: DestinationHierarchyWorkflowIslandProps) {
  const copy = getDestinationHierarchyWorkflowCopy(locale);
  const formId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);

  const [countries, setCountries] = useState<CountryCatalogView[]>([]);
  const [countryFilter, setCountryFilter] = useState("");
  const [focused, setFocused] = useState<DestinationSummaryView | null>(null);
  const [path, setPath] = useState<DestinationPathView | null>(null);
  const [children, setChildren] = useState<DestinationSummaryView[]>([]);
  const [descendants, setDescendants] = useState<DestinationPathNodeView[]>([]);
  const [translations, setTranslations] = useState<DestinationTranslationView[]>(
    [],
  );

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

  function mapAuthError(status?: number, notFound = false) {
    if (status === 401 || status === 403) return copy.unauthorizedBody;
    if (notFound || status === 404) return copy.notFound;
    return copy.errorGeneric;
  }

  async function focusDestination(destinationId: string) {
    const bundle = await loadDestinationBundleAction(
      destinationId,
      locale === "ar" ? "en" : locale,
    );
    if (!bundle.ok) {
      setError(mapAuthError(bundle.status, bundle.status === 404));
      return;
    }
    setFocused(bundle.destination);
    setPath(bundle.path);
    setChildren(bundle.children);
    setDescendants(bundle.descendants);
    setTranslations(bundle.translations);
  }

  useEffect(() => {
    if (!apiConfigured) return;
    let cancelled = false;
    void (async () => {
      const result = await listCountriesAction();
      if (cancelled) return;
      if (result.ok) setCountries(result.items);
    })();
    return () => {
      cancelled = true;
    };
  }, [apiConfigured]);

  const filteredCountries = useMemo(() => {
    const q = countryFilter.trim().toLowerCase();
    if (!q) return countries.slice(0, 40);
    return countries
      .filter(
        (c) =>
          c.englishName.toLowerCase().includes(q) ||
          c.alpha2Code.toLowerCase().includes(q) ||
          c.alpha3Code.toLowerCase().includes(q),
      )
      .slice(0, 40);
  }, [countries, countryFilter]);

  const createKinds = childKindsFor(focused?.kind ?? null);

  if (!apiConfigured) {
    return (
      <Surface tone="muted">
        <Text role="muted">{copy.apiMissing}</Text>
      </Surface>
    );
  }

  return (
    <Stack gap="lg">
      {error ? (
        <Surface tone="muted">
          <Text role="muted">{error}</Text>
        </Surface>
      ) : null}

      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.stepOpen}
          </Text>
          <form
            id={`${formId}-open`}
            className="flex flex-col gap-3"
            onSubmit={(e) => {
              e.preventDefault();
              const fd = new FormData(e.currentTarget);
              const slugLocale = String(fd.get("slugLocale") ?? "fa");
              const slug = String(fd.get("slug") ?? "");
              run(async () => {
                const result = await openBySlugAction({
                  localeCode: slugLocale,
                  slug,
                });
                if (!result.ok) {
                  setError(mapAuthError(result.status, result.status === 404));
                  return;
                }
                await focusDestination(result.destination.id);
              });
            }}
          >
            <Text role="muted">{copy.openBySlugLabel}</Text>
            <label className="flex flex-col gap-1 text-sm">
              <span>{copy.slugLocaleLabel}</span>
              <select
                name="slugLocale"
                defaultValue={locale === "ar" ? "en" : locale}
                className="min-h-touch rounded-md border border-border bg-background px-3"
              >
                <option value="fa">fa</option>
                <option value="en">en</option>
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span>{copy.slugValueLabel}</span>
              <input
                name="slug"
                required
                className="min-h-touch rounded-md border border-border bg-background px-3"
              />
            </label>
            <button
              type="submit"
              disabled={pending}
              className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
            >
              {copy.openBySlug}
            </button>
          </form>

          {!focused ? (
            <form
              id={`${formId}-root`}
              className="mt-4 flex flex-col gap-3 border-t border-border pt-4"
              onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                const code = String(fd.get("code") ?? "");
                const englishName = String(fd.get("englishName") ?? "");
                const isoCountryCode = String(fd.get("isoCountryCode") ?? "");
                run(async () => {
                  const result = await createDestinationAction({
                    kind: "Country",
                    code,
                    englishName,
                    parentId: null,
                    isoCountryCode,
                  });
                  if (!result.ok) {
                    setError(mapAuthError(result.status));
                    return;
                  }
                  await focusDestination(result.destination.id);
                });
              }}
            >
              <Text as="h3" role="heading">
                {copy.createRootCountry}
              </Text>
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.codeLabel}</span>
                <input
                  name="code"
                  required
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.englishNameLabel}</span>
                <input
                  name="englishName"
                  required
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.isoCountryFilter}</span>
                <input
                  value={countryFilter}
                  onChange={(e) => setCountryFilter(e.target.value)}
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.isoCountryLabel}</span>
                <select
                  name="isoCountryCode"
                  required
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                >
                  <option value="">{copy.noCountries}</option>
                  {filteredCountries.map((c) => (
                    <option key={c.alpha2Code} value={c.alpha2Code}>
                      {c.englishName} ({c.alpha2Code})
                    </option>
                  ))}
                </select>
              </label>
              <button
                type="submit"
                disabled={pending || filteredCountries.length === 0}
                className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
              >
                {copy.createDestination}
              </button>
            </form>
          ) : null}
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.stepBrowse}
          </Text>
          {!focused || !path ? (
            <Text role="muted">{copy.noFocus}</Text>
          ) : (
            <>
              <Text role="muted">
                {copy.focusedTitle}: {focused.englishName} (
                {kindLabel(focused.kind, copy)}) ·{" "}
                <LtrValue>{focused.code}</LtrValue>
                {focused.isoCountryCode ? (
                  <>
                    {" "}
                    · ISO <LtrValue>{focused.isoCountryCode}</LtrValue>
                  </>
                ) : null}
              </Text>
              <div>
                <Text role="caption">{copy.breadcrumbLabel}</Text>
                <ol className="mt-2 flex flex-wrap items-center gap-2 text-sm">
                  {path.breadcrumbRootFirst.map((node, index) => (
                    <li key={node.id} className="inline-flex items-center gap-2">
                      {index > 0 ? <span aria-hidden="true">/</span> : null}
                      <button
                        type="button"
                        disabled={pending || node.id === focused.id}
                        className="min-h-touch underline-offset-2 hover:underline disabled:no-underline disabled:opacity-60"
                        onClick={() =>
                          run(async () => {
                            await focusDestination(node.id);
                          })
                        }
                      >
                        {node.englishName}
                      </button>
                    </li>
                  ))}
                </ol>
              </div>
              <div>
                <Text role="caption">{copy.childrenLabel}</Text>
                {children.length === 0 ? (
                  <Text role="muted">{copy.noChildren}</Text>
                ) : (
                  <ul className="mt-2 flex flex-col gap-2">
                    {children.map((child) => (
                      <li
                        key={child.id}
                        className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border px-3 py-2"
                      >
                        <span className="text-sm">
                          {child.englishName} ({kindLabel(child.kind, copy)}) ·{" "}
                          <LtrValue>{child.code}</LtrValue>
                        </span>
                        <button
                          type="button"
                          disabled={pending}
                          className="min-h-touch rounded-md border border-border px-3 disabled:opacity-50"
                          onClick={() =>
                            run(async () => {
                              await focusDestination(child.id);
                            })
                          }
                        >
                          {copy.focusChild}
                        </button>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
              {descendants.length > 0 ? (
                <div>
                  <Text role="caption">{copy.descendantsLabel}</Text>
                  <ul className="mt-2 list-inside list-disc text-sm">
                    {descendants.map((n) => (
                      <li key={`${n.id}-${n.depthFromRoot}`}>
                        {n.englishName} ({kindLabel(n.kind, copy)})
                      </li>
                    ))}
                  </ul>
                </div>
              ) : null}
            </>
          )}
        </Stack>
      </Surface>

      {focused && createKinds.length > 0 && createKinds[0] !== "Country" ? (
        <Surface>
          <Stack gap="sm">
            <Text as="h2" role="heading">
              {copy.stepCreate}
            </Text>
            <Text role="muted">
              {copy.parentContext}: {focused.englishName}
            </Text>
            <form
              id={`${formId}-create`}
              className="flex flex-col gap-3"
              onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                const kind = String(fd.get("kind") ?? "");
                const code = String(fd.get("code") ?? "");
                const englishName = String(fd.get("englishName") ?? "");
                run(async () => {
                  const result = await createDestinationAction({
                    kind,
                    code,
                    englishName,
                    parentId: focused.id,
                    isoCountryCode: null,
                  });
                  if (!result.ok) {
                    setError(mapAuthError(result.status));
                    return;
                  }
                  await focusDestination(focused.id);
                });
              }}
            >
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.kindLabel}</span>
                <select
                  name="kind"
                  required
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                >
                  {createKinds.map((k) => (
                    <option key={k} value={k}>
                      {kindLabel(k, copy)}
                    </option>
                  ))}
                </select>
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.codeLabel}</span>
                <input
                  name="code"
                  required
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.englishNameLabel}</span>
                <input
                  name="englishName"
                  required
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
              <button
                type="submit"
                disabled={pending}
                className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
              >
                {copy.createDestination}
              </button>
            </form>
          </Stack>
        </Surface>
      ) : null}

      {focused ? (
        <Surface>
          <Stack gap="sm">
            <Text as="h2" role="heading">
              {copy.stepTranslate}
            </Text>
            <Text role="caption">{copy.slugNotSeo}</Text>
            {translations.length > 0 ? (
              <ul className="text-sm">
                {translations.map((t) => (
                  <li key={`${t.localeCode}-${t.slug ?? "none"}`}>
                    <LtrValue>{t.localeCode}</LtrValue>: {t.name}
                    {t.slug ? (
                      <>
                        {" "}
                        · <LtrValue>{t.slug}</LtrValue>
                      </>
                    ) : null}
                  </li>
                ))}
              </ul>
            ) : null}
            <form
              id={`${formId}-translate`}
              className="flex flex-col gap-3"
              onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                const localeCode = String(fd.get("localeCode") ?? "fa");
                const name = String(fd.get("name") ?? "");
                const description = String(fd.get("description") ?? "");
                const slug = String(fd.get("slug") ?? "").trim();
                run(async () => {
                  const result = await upsertTranslationAction({
                    destinationId: focused.id,
                    localeCode,
                    name,
                    description: description || null,
                    slug: slug || null,
                  });
                  if (!result.ok) {
                    setError(mapAuthError(result.status));
                    return;
                  }
                  if (slug) {
                    const slugResult = await setTranslationSlugAction({
                      destinationId: focused.id,
                      localeCode,
                      slug,
                    });
                    if (!slugResult.ok) {
                      setError(mapAuthError(slugResult.status));
                      return;
                    }
                  }
                  await focusDestination(focused.id);
                });
              }}
            >
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.translationLocale}</span>
                <select
                  name="localeCode"
                  defaultValue={locale === "ar" ? "fa" : locale}
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                >
                  <option value="fa">fa</option>
                  <option value="en">en</option>
                </select>
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.translationName}</span>
                <input
                  name="name"
                  required
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.translationDescription}</span>
                <textarea
                  name="description"
                  rows={3}
                  className="rounded-md border border-border bg-background px-3 py-2"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.translationSlug}</span>
                <input
                  name="slug"
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
              <button
                type="submit"
                disabled={pending}
                className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
              >
                {copy.saveTranslation}
              </button>
            </form>
          </Stack>
        </Surface>
      ) : null}

      {focused ? (
        <Surface>
          <Stack gap="sm">
            <Text as="h2" role="heading">
              {copy.stepGeo}
            </Text>
            <Text role="muted">
              {focused.latitude != null && focused.longitude != null ? (
                <>
                  <LtrValue>{String(focused.latitude)}</LtrValue>,{" "}
                  <LtrValue>{String(focused.longitude)}</LtrValue>
                </>
              ) : (
                "—"
              )}
            </Text>
            <form
              id={`${formId}-geo`}
              className="flex flex-col gap-3"
              onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                const latRaw = String(fd.get("latitude") ?? "").trim();
                const lngRaw = String(fd.get("longitude") ?? "").trim();
                const latitude = latRaw === "" ? null : Number(latRaw);
                const longitude = lngRaw === "" ? null : Number(lngRaw);
                run(async () => {
                  const result = await setGeoAction({
                    destinationId: focused.id,
                    latitude,
                    longitude,
                  });
                  if (!result.ok) {
                    setError(mapAuthError(result.status));
                    return;
                  }
                  await focusDestination(focused.id);
                });
              }}
            >
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.latitudeLabel}</span>
                <input
                  name="latitude"
                  type="number"
                  step="any"
                  defaultValue={focused.latitude ?? ""}
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.longitudeLabel}</span>
                <input
                  name="longitude"
                  type="number"
                  step="any"
                  defaultValue={focused.longitude ?? ""}
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
              <div className="flex flex-wrap gap-2">
                <button
                  type="submit"
                  disabled={pending}
                  className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                >
                  {copy.saveGeo}
                </button>
                <button
                  type="button"
                  disabled={pending}
                  className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                  onClick={() =>
                    run(async () => {
                      const result = await setGeoAction({
                        destinationId: focused.id,
                        latitude: null,
                        longitude: null,
                      });
                      if (!result.ok) {
                        setError(mapAuthError(result.status));
                        return;
                      }
                      await focusDestination(focused.id);
                    })
                  }
                >
                  {copy.clearGeo}
                </button>
              </div>
            </form>
          </Stack>
        </Surface>
      ) : null}
    </Stack>
  );
}
