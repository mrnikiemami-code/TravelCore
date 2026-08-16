"use client";

import { useId, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import {
  addPlaceGalleryItemAction,
  createPlaceAction,
  listPlacesAction,
  loadPlaceDetailAction,
  openPlaceByCodeAction,
  removePlaceCoverAction,
  removePlaceGalleryItemAction,
  resolveDestinationBySlugAction,
  setPlaceAddressAction,
  setPlaceCatalogFieldsAction,
  setPlaceCoverAction,
  setPlaceDestinationLinkAction,
  setPlaceGeoAction,
  upsertPlaceTranslationAction,
} from "@/features/admin-place/actions";
import { getAdminPlaceWorkflowCopy } from "@/features/admin-place/copy";
import type {
  PlaceDetailView,
  PlaceSummaryView,
} from "@/features/admin-place/types";

export type PlaceWorkflowIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

export function PlaceWorkflowIsland({
  locale,
  apiConfigured,
}: PlaceWorkflowIslandProps) {
  const copy = getAdminPlaceWorkflowCopy(locale);
  const formId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [kindFilter, setKindFilter] = useState("");
  const [take, setTake] = useState(50);
  const [items, setItems] = useState<PlaceSummaryView[]>([]);
  const [detail, setDetail] = useState<PlaceDetailView | null>(null);
  const [openCode, setOpenCode] = useState("");
  const [createKind, setCreateKind] = useState<"Hotel" | "Restaurant" | "Attraction">(
    "Hotel",
  );
  const [createCode, setCreateCode] = useState("");
  const [createName, setCreateName] = useState("");
  const [starRating, setStarRating] = useState("4");
  const [cuisineType, setCuisineType] = useState("");
  const [categoryCode, setCategoryCode] = useState("");
  const [translationLocale, setTranslationLocale] = useState<"fa" | "en">(
    locale === "en" ? "en" : "fa",
  );
  const [localizedName, setLocalizedName] = useState("");
  const [localizedDescription, setLocalizedDescription] = useState("");
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
  const [latitude, setLatitude] = useState("");
  const [longitude, setLongitude] = useState("");
  const [line1, setLine1] = useState("");
  const [line2, setLine2] = useState("");
  const [locality, setLocality] = useState("");
  const [adminArea, setAdminArea] = useState("");
  const [postalCode, setPostalCode] = useState("");
  const [countryCode, setCountryCode] = useState("");
  const [catalogStatus, setCatalogStatus] = useState("Draft");
  const [classificationCode, setClassificationCode] = useState("");
  const [facilitiesText, setFacilitiesText] = useState("");
  const [coverMediaId, setCoverMediaId] = useState("");
  const [galleryMediaId, setGalleryMediaId] = useState("");

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

  function applyDetail(next: PlaceDetailView) {
    setDetail(next);
    const place = next.place;
    const row = next.translations.find(
      (t) => t.localeCode.toLowerCase() === translationLocale,
    );
    setLocalizedName(row?.name ?? "");
    setLocalizedDescription(row?.description ?? "");
    setLatitude(place.latitude == null ? "" : String(place.latitude));
    setLongitude(place.longitude == null ? "" : String(place.longitude));
    setLine1(place.address?.line1 ?? "");
    setLine2(place.address?.line2 ?? "");
    setLocality(place.address?.locality ?? "");
    setAdminArea(place.address?.administrativeArea ?? "");
    setPostalCode(place.address?.postalCode ?? "");
    setCountryCode(place.address?.countryCode ?? "");
    setCatalogStatus(place.catalogStatus || "Draft");
    setClassificationCode(place.classificationCode ?? "");
    setFacilitiesText(place.facilities.join(", "));
    setResolvedDestination(null);
    setDestSlug("");
    setCoverMediaId("");
    setGalleryMediaId("");
  }

  async function refreshList() {
    const result = await listPlacesAction({
      kind: kindFilter || undefined,
      take,
    });
    if (!result.ok) {
      setError(mapAuthError(result.status));
      return;
    }
    setItems(result.items);
  }

  async function openPlace(id: string) {
    const result = await loadPlaceDetailAction(id);
    if (!result.ok) {
      setError(mapAuthError(result.status));
      setDetail(null);
      return;
    }
    applyDetail(result.detail);
  }

  async function reloadSelected() {
    if (!detail) return;
    await openPlace(detail.place.id);
  }

  if (!apiConfigured) {
    return (
      <Surface tone="muted">
        <Text role="muted">{copy.apiMissing}</Text>
      </Surface>
    );
  }

  const selected = detail?.place ?? null;

  return (
    <Stack gap="lg">
      {error ? (
        <div role="alert">
          <Surface tone="muted">
            <Text role="muted">{error}</Text>
          </Surface>
        </div>
      ) : null}

      <Surface tone="muted">
        <Text role="caption">{copy.noDeleteHint}</Text>
      </Surface>

      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.stepCreate}
          </Text>
          <form
            id={`${formId}-create`}
            className="grid gap-3 sm:grid-cols-2"
            onSubmit={(e) => {
              e.preventDefault();
              run(async () => {
                const result = await createPlaceAction({
                  kind: createKind,
                  code: createCode,
                  englishName: createName,
                  starRating:
                    createKind === "Hotel"
                      ? Number(starRating) || null
                      : null,
                  cuisineType:
                    createKind === "Restaurant" ? cuisineType || null : null,
                  categoryCode:
                    createKind === "Attraction" ? categoryCode || null : null,
                });
                if (!result.ok) {
                  setError(mapAuthError(result.status));
                  return;
                }
                setCreateCode("");
                setCreateName("");
                await refreshList();
                await openPlace(result.place.id);
              });
            }}
          >
            <label className="flex flex-col gap-1 text-sm">
              <span>{copy.kindLabel}</span>
              <select
                value={createKind}
                onChange={(e) =>
                  setCreateKind(
                    e.target.value as "Hotel" | "Restaurant" | "Attraction",
                  )
                }
                className="min-h-touch rounded-md border border-border bg-background px-3"
              >
                <option value="Hotel">Hotel</option>
                <option value="Restaurant">Restaurant</option>
                <option value="Attraction">Attraction</option>
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span>{copy.codeLabel}</span>
              <input
                required
                value={createCode}
                onChange={(e) => setCreateCode(e.target.value)}
                className="min-h-touch rounded-md border border-border bg-background px-3"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm sm:col-span-2">
              <span>{copy.englishNameLabel}</span>
              <input
                required
                value={createName}
                onChange={(e) => setCreateName(e.target.value)}
                className="min-h-touch rounded-md border border-border bg-background px-3"
              />
            </label>
            {createKind === "Hotel" ? (
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.starRatingLabel}</span>
                <input
                  type="number"
                  min={1}
                  max={5}
                  value={starRating}
                  onChange={(e) => setStarRating(e.target.value)}
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
            ) : null}
            {createKind === "Restaurant" ? (
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.cuisineTypeLabel}</span>
                <input
                  value={cuisineType}
                  onChange={(e) => setCuisineType(e.target.value)}
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
            ) : null}
            {createKind === "Attraction" ? (
              <label className="flex flex-col gap-1 text-sm">
                <span>{copy.categoryCodeLabel}</span>
                <input
                  value={categoryCode}
                  onChange={(e) => setCategoryCode(e.target.value)}
                  className="min-h-touch rounded-md border border-border bg-background px-3"
                />
              </label>
            ) : null}
            <div className="sm:col-span-2">
              <button
                type="submit"
                disabled={pending}
                className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
              >
                {copy.createAction}
              </button>
            </div>
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
              <span>{copy.kindFilterLabel}</span>
              <select
                value={kindFilter}
                onChange={(e) => setKindFilter(e.target.value)}
                className="min-h-touch rounded-md border border-border bg-background px-3"
              >
                <option value="">{copy.kindAll}</option>
                <option value="Hotel">Hotel</option>
                <option value="Restaurant">Restaurant</option>
                <option value="Attraction">Attraction</option>
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

          <form
            className="flex flex-col gap-3 sm:flex-row sm:items-end"
            onSubmit={(e) => {
              e.preventDefault();
              run(async () => {
                const result = await openPlaceByCodeAction({
                  code: openCode,
                  locale: translationLocale,
                });
                if (!result.ok) {
                  setError(mapAuthError(result.status));
                  return;
                }
                applyDetail(result.detail);
              });
            }}
          >
            <label className="flex min-w-0 flex-1 flex-col gap-1 text-sm">
              <span>{copy.stepOpenByCode}</span>
              <input
                value={openCode}
                onChange={(e) => setOpenCode(e.target.value)}
                required
                className="min-h-touch rounded-md border border-border bg-background px-3"
              />
              <Text role="caption">{copy.openByCodeHint}</Text>
            </label>
            <button
              type="submit"
              disabled={pending}
              className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
            >
              {copy.openByCodeAction}
            </button>
          </form>

          {items.length === 0 ? (
            <Text role="caption">{copy.noPlaces}</Text>
          ) : (
            <ul className="flex flex-col gap-2">
              {items.map((item) => (
                <li
                  key={item.id}
                  className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border px-3 py-2"
                >
                  <div className="min-w-0">
                    <Text as="p">
                      {item.englishName}
                      {" · "}
                      <LtrValue>{item.code}</LtrValue>
                      {" · "}
                      {item.kind}
                      {" · "}
                      {item.catalogStatus}
                    </Text>
                  </div>
                  <button
                    type="button"
                    disabled={pending}
                    className="min-h-touch rounded-md border border-border px-3 disabled:opacity-50"
                    onClick={() =>
                      run(async () => {
                        await openPlace(item.id);
                      })
                    }
                  >
                    {copy.selectPlace}
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
              <dl className="flex flex-col gap-1 text-sm">
                <div>
                  <dt className="inline font-medium">{copy.englishNameLabel}: </dt>
                  <dd className="inline">{selected.englishName}</dd>
                </div>
                <div>
                  <dt className="inline font-medium">{copy.codeLabel}: </dt>
                  <dd className="inline">
                    <LtrValue>{selected.code}</LtrValue>
                  </dd>
                </div>
                <div>
                  <dt className="inline font-medium">{copy.kindLabel}: </dt>
                  <dd className="inline">{selected.kind}</dd>
                </div>
                <div>
                  <dt className="inline font-medium">{copy.statusLabel}: </dt>
                  <dd className="inline">{selected.catalogStatus}</dd>
                </div>
              </dl>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.stepTranslate}
              </Text>
              <form
                className="grid gap-3 sm:grid-cols-2"
                onSubmit={(e) => {
                  e.preventDefault();
                  run(async () => {
                    const result = await upsertPlaceTranslationAction({
                      placeId: selected.id,
                      localeCode: translationLocale,
                      name: localizedName,
                      description: localizedDescription || null,
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status));
                      return;
                    }
                    await reloadSelected();
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
                      setLocalizedName(row?.name ?? "");
                      setLocalizedDescription(row?.description ?? "");
                    }}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  >
                    <option value="fa">fa</option>
                    <option value="en">en</option>
                  </select>
                </label>
                <label className="flex flex-col gap-1 text-sm sm:col-span-2">
                  <span>{copy.nameLabel}</span>
                  <input
                    required
                    value={localizedName}
                    onChange={(e) => setLocalizedName(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm sm:col-span-2">
                  <span>{copy.descriptionLabel}</span>
                  <textarea
                    value={localizedDescription}
                    onChange={(e) => setLocalizedDescription(e.target.value)}
                    rows={3}
                    className="rounded-md border border-border bg-background px-3 py-2"
                  />
                </label>
                <div>
                  <button
                    type="submit"
                    disabled={pending}
                    className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                  >
                    {copy.saveTranslation}
                  </button>
                </div>
              </form>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.stepDestination}
              </Text>
              <form
                className="grid gap-3 sm:grid-cols-2"
                onSubmit={(e) => {
                  e.preventDefault();
                  run(async () => {
                    const hit = await resolveDestinationBySlugAction({
                      localeCode: destSlugLocale,
                      slug: destSlug,
                    });
                    if (!hit.ok) {
                      setError(mapAuthError(hit.status));
                      return;
                    }
                    setResolvedDestination(hit.destination);
                  });
                }}
              >
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.destinationSlugLocale}</span>
                  <select
                    value={destSlugLocale}
                    onChange={(e) =>
                      setDestSlugLocale(e.target.value as "fa" | "en")
                    }
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  >
                    <option value="fa">fa</option>
                    <option value="en">en</option>
                  </select>
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.destinationSlugLabel}</span>
                  <input
                    value={destSlug}
                    onChange={(e) => setDestSlug(e.target.value)}
                    required
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <div className="sm:col-span-2">
                  <button
                    type="submit"
                    disabled={pending}
                    className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                  >
                    {copy.resolveDestination}
                  </button>
                </div>
              </form>
              {resolvedDestination ? (
                <Text role="caption">
                  {copy.destinationResolved}: {resolvedDestination.englishName}{" "}
                  · <LtrValue>{resolvedDestination.code}</LtrValue>
                </Text>
              ) : selected.destinationId ? (
                <Text role="caption">
                  {copy.destinationResolved}:{" "}
                  <LtrValue>{selected.destinationId}</LtrValue>
                </Text>
              ) : null}
              <div className="flex flex-wrap gap-2">
                <button
                  type="button"
                  disabled={pending || !resolvedDestination}
                  className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                  onClick={() =>
                    run(async () => {
                      if (!resolvedDestination) return;
                      const result = await setPlaceDestinationLinkAction({
                        placeId: selected.id,
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
                  disabled={pending}
                  className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                  onClick={() =>
                    run(async () => {
                      const result = await setPlaceDestinationLinkAction({
                        placeId: selected.id,
                        destinationId: null,
                      });
                      if (!result.ok) {
                        setError(mapAuthError(result.status));
                        return;
                      }
                      setResolvedDestination(null);
                      await reloadSelected();
                    })
                  }
                >
                  {copy.clearDestination}
                </button>
              </div>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.stepGeo}
              </Text>
              <form
                className="grid gap-3 sm:grid-cols-2"
                onSubmit={(e) => {
                  e.preventDefault();
                  run(async () => {
                    const result = await setPlaceGeoAction({
                      placeId: selected.id,
                      latitude: latitude.trim() ? Number(latitude) : null,
                      longitude: longitude.trim() ? Number(longitude) : null,
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status));
                      return;
                    }
                    await reloadSelected();
                  });
                }}
              >
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.latitudeLabel}</span>
                  <input
                    value={latitude}
                    onChange={(e) => setLatitude(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.longitudeLabel}</span>
                  <input
                    value={longitude}
                    onChange={(e) => setLongitude(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <div className="flex flex-wrap gap-2 sm:col-span-2">
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
                        const result = await setPlaceGeoAction({
                          placeId: selected.id,
                          latitude: null,
                          longitude: null,
                        });
                        if (!result.ok) {
                          setError(mapAuthError(result.status));
                          return;
                        }
                        setLatitude("");
                        setLongitude("");
                        await reloadSelected();
                      })
                    }
                  >
                    {copy.clearGeo}
                  </button>
                </div>
              </form>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.stepAddress}
              </Text>
              <form
                className="grid gap-3 sm:grid-cols-2"
                onSubmit={(e) => {
                  e.preventDefault();
                  run(async () => {
                    const result = await setPlaceAddressAction({
                      placeId: selected.id,
                      line1: line1 || null,
                      line2: line2 || null,
                      locality: locality || null,
                      administrativeArea: adminArea || null,
                      postalCode: postalCode || null,
                      countryCode: countryCode || null,
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
                  <span>{copy.line1Label}</span>
                  <input
                    value={line1}
                    onChange={(e) => setLine1(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm sm:col-span-2">
                  <span>{copy.line2Label}</span>
                  <input
                    value={line2}
                    onChange={(e) => setLine2(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.localityLabel}</span>
                  <input
                    value={locality}
                    onChange={(e) => setLocality(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.adminAreaLabel}</span>
                  <input
                    value={adminArea}
                    onChange={(e) => setAdminArea(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.postalCodeLabel}</span>
                  <input
                    value={postalCode}
                    onChange={(e) => setPostalCode(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.countryCodeLabel}</span>
                  <input
                    value={countryCode}
                    onChange={(e) => setCountryCode(e.target.value)}
                    maxLength={2}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <div className="flex flex-wrap gap-2 sm:col-span-2">
                  <button
                    type="submit"
                    disabled={pending}
                    className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                  >
                    {copy.saveAddress}
                  </button>
                  <button
                    type="button"
                    disabled={pending}
                    className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                    onClick={() =>
                      run(async () => {
                        const result = await setPlaceAddressAction({
                          placeId: selected.id,
                          line1: null,
                          line2: null,
                          locality: null,
                          administrativeArea: null,
                          postalCode: null,
                          countryCode: null,
                        });
                        if (!result.ok) {
                          setError(mapAuthError(result.status));
                          return;
                        }
                        setLine1("");
                        setLine2("");
                        setLocality("");
                        setAdminArea("");
                        setPostalCode("");
                        setCountryCode("");
                        await reloadSelected();
                      })
                    }
                  >
                    {copy.clearAddress}
                  </button>
                </div>
              </form>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.stepCatalog}
              </Text>
              <form
                className="grid gap-3 sm:grid-cols-2"
                onSubmit={(e) => {
                  e.preventDefault();
                  run(async () => {
                    const facilityCodes = facilitiesText
                      .split(",")
                      .map((x) => x.trim())
                      .filter(Boolean);
                    const result = await setPlaceCatalogFieldsAction({
                      placeId: selected.id,
                      catalogStatus,
                      classificationCode: classificationCode.trim() || null,
                      facilityCodes,
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status));
                      return;
                    }
                    await reloadSelected();
                  });
                }}
              >
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.statusLabel}</span>
                  <select
                    value={catalogStatus}
                    onChange={(e) => setCatalogStatus(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  >
                    <option value="Draft">{copy.catalogDraft}</option>
                    <option value="Active">{copy.catalogActive}</option>
                    <option value="Inactive">{copy.catalogInactive}</option>
                  </select>
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span>{copy.classificationLabel}</span>
                  <input
                    value={classificationCode}
                    onChange={(e) => setClassificationCode(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm sm:col-span-2">
                  <span>{copy.facilitiesLabel}</span>
                  <input
                    value={facilitiesText}
                    onChange={(e) => setFacilitiesText(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                  <Text role="caption">{copy.facilitiesHint}</Text>
                </label>
                <div>
                  <button
                    type="submit"
                    disabled={pending}
                    className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                  >
                    {copy.saveCatalog}
                  </button>
                </div>
              </form>
            </Stack>
          </Surface>

          <Surface>
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {copy.stepMedia}
              </Text>
              <form
                className="flex flex-col gap-3 sm:flex-row sm:items-end"
                onSubmit={(e) => {
                  e.preventDefault();
                  run(async () => {
                    const result = await setPlaceCoverAction({
                      placeId: selected.id,
                      mediaAssetId: coverMediaId.trim(),
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status));
                      return;
                    }
                    await reloadSelected();
                  });
                }}
              >
                <label className="flex min-w-0 flex-1 flex-col gap-1 text-sm">
                  <span>{copy.coverMediaLabel}</span>
                  <input
                    required
                    value={coverMediaId}
                    onChange={(e) => setCoverMediaId(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <button
                  type="submit"
                  disabled={pending}
                  className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                >
                  {copy.setCover}
                </button>
                <button
                  type="button"
                  disabled={pending}
                  className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                  onClick={() =>
                    run(async () => {
                      const result = await removePlaceCoverAction(selected.id);
                      if (!result.ok) {
                        setError(mapAuthError(result.status));
                        return;
                      }
                      await reloadSelected();
                    })
                  }
                >
                  {copy.removeCover}
                </button>
              </form>

              <form
                className="flex flex-col gap-3 sm:flex-row sm:items-end"
                onSubmit={(e) => {
                  e.preventDefault();
                  run(async () => {
                    const result = await addPlaceGalleryItemAction({
                      placeId: selected.id,
                      mediaAssetId: galleryMediaId.trim(),
                    });
                    if (!result.ok) {
                      setError(mapAuthError(result.status));
                      return;
                    }
                    setGalleryMediaId("");
                    await reloadSelected();
                  });
                }}
              >
                <label className="flex min-w-0 flex-1 flex-col gap-1 text-sm">
                  <span>{copy.galleryMediaLabel}</span>
                  <input
                    required
                    value={galleryMediaId}
                    onChange={(e) => setGalleryMediaId(e.target.value)}
                    className="min-h-touch rounded-md border border-border bg-background px-3"
                  />
                </label>
                <button
                  type="submit"
                  disabled={pending}
                  className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                >
                  {copy.addGallery}
                </button>
              </form>

              <Text as="h3" role="heading">
                {copy.mediaLinksHeading}
              </Text>
              {!detail?.mediaLinks.length ? (
                <Text role="caption">{copy.noMediaLinks}</Text>
              ) : (
                <ul className="flex flex-col gap-2">
                  {detail.mediaLinks.map((link) => (
                    <li
                      key={`${link.role}-${link.mediaAssetId}`}
                      className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border px-3 py-2"
                    >
                      <Text as="p">
                        {link.role}
                        {" · "}
                        <LtrValue>{link.mediaAssetId}</LtrValue>
                        {link.role === "Gallery" ? (
                          <>
                            {" · #"}
                            {link.sortOrder}
                          </>
                        ) : null}
                      </Text>
                      {link.role === "Gallery" ? (
                        <button
                          type="button"
                          disabled={pending}
                          className="min-h-touch rounded-md border border-border px-3 disabled:opacity-50"
                          onClick={() =>
                            run(async () => {
                              const result = await removePlaceGalleryItemAction({
                                placeId: selected.id,
                                mediaAssetId: link.mediaAssetId,
                              });
                              if (!result.ok) {
                                setError(mapAuthError(result.status));
                                return;
                              }
                              await reloadSelected();
                            })
                          }
                        >
                          {copy.removeGalleryItem}
                        </button>
                      ) : null}
                    </li>
                  ))}
                </ul>
              )}
            </Stack>
          </Surface>
        </>
      ) : null}
    </Stack>
  );
}
