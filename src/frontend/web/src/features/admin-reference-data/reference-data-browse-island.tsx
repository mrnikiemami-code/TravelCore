"use client";

import { useEffect, useMemo, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import {
  listCountriesAction,
  listCurrenciesAction,
  listLocalesAction,
} from "@/features/admin-reference-data/actions";
import { getReferenceDataAdminCopy } from "@/features/admin-reference-data/copy";
import type {
  CountryCatalogView,
  CurrencyCatalogView,
  LocaleCatalogView,
} from "@/features/admin-reference-data/types";

export type ReferenceDataBrowseIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

export function ReferenceDataBrowseIsland({
  locale,
  apiConfigured,
}: ReferenceDataBrowseIslandProps) {
  const copy = getReferenceDataAdminCopy(locale);
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState("");
  const [countries, setCountries] = useState<CountryCatalogView[]>([]);
  const [currencies, setCurrencies] = useState<CurrencyCatalogView[]>([]);
  const [locales, setLocales] = useState<LocaleCatalogView[]>([]);

  useEffect(() => {
    if (!apiConfigured) return;
    startTransition(async () => {
      setError(null);
      const [c, cur, loc] = await Promise.all([
        listCountriesAction(),
        listCurrenciesAction(),
        listLocalesAction(),
      ]);
      if (!c.ok || !cur.ok || !loc.ok) {
        setError(copy.errorGeneric);
        return;
      }
      setCountries(c.items);
      setCurrencies(cur.items);
      setLocales(loc.items);
    });
  }, [apiConfigured, copy.errorGeneric]);

  const q = filter.trim().toLowerCase();
  const filteredCountries = useMemo(() => {
    const list = !q
      ? countries
      : countries.filter(
          (c) =>
            c.englishName.toLowerCase().includes(q) ||
            c.alpha2Code.toLowerCase().includes(q) ||
            c.alpha3Code.toLowerCase().includes(q),
        );
    return list.slice(0, 60);
  }, [countries, q]);

  const filteredCurrencies = useMemo(() => {
    const list = !q
      ? currencies
      : currencies.filter(
          (c) =>
            c.englishName.toLowerCase().includes(q) ||
            c.code.toLowerCase().includes(q),
        );
    return list.slice(0, 40);
  }, [currencies, q]);

  const filteredLocales = useMemo(() => {
    const list = !q
      ? locales
      : locales.filter(
          (l) =>
            l.englishName.toLowerCase().includes(q) ||
            l.code.toLowerCase().includes(q),
        );
    return list.slice(0, 40);
  }, [locales, q]);

  if (!apiConfigured) {
    return (
      <Surface tone="muted">
        <Text role="muted">{copy.apiMissing}</Text>
      </Surface>
    );
  }

  return (
    <Stack gap="lg">
      <Text role="caption">{copy.readOnlyNote}</Text>
      {error ? (
        <Surface tone="muted">
          <Text role="muted">{error}</Text>
        </Surface>
      ) : null}
      <label className="flex flex-col gap-1 text-sm">
        <span>{copy.filterLabel}</span>
        <input
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          disabled={pending}
          className="min-h-touch rounded-md border border-border bg-background px-3 disabled:opacity-50"
        />
      </label>

      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.countriesHeading}
          </Text>
          {filteredCountries.length === 0 ? (
            <Text role="muted">{copy.empty}</Text>
          ) : (
            <ul className="flex flex-col gap-2 text-sm">
              {filteredCountries.map((c) => (
                <li
                  key={c.alpha2Code}
                  className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border px-3 py-2"
                >
                  <span>{c.englishName}</span>
                  <LtrValue>
                    {c.alpha2Code} / {c.alpha3Code}
                  </LtrValue>
                </li>
              ))}
            </ul>
          )}
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.currenciesHeading}
          </Text>
          {filteredCurrencies.length === 0 ? (
            <Text role="muted">{copy.empty}</Text>
          ) : (
            <ul className="flex flex-col gap-2 text-sm">
              {filteredCurrencies.map((c) => (
                <li
                  key={c.code}
                  className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border px-3 py-2"
                >
                  <span>{c.englishName}</span>
                  <LtrValue>{c.code}</LtrValue>
                </li>
              ))}
            </ul>
          )}
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.localesHeading}
          </Text>
          {filteredLocales.length === 0 ? (
            <Text role="muted">{copy.empty}</Text>
          ) : (
            <ul className="flex flex-col gap-2 text-sm">
              {filteredLocales.map((l) => (
                <li
                  key={l.code}
                  className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border px-3 py-2"
                >
                  <span>{l.englishName}</span>
                  <LtrValue>{l.code}</LtrValue>
                </li>
              ))}
            </ul>
          )}
        </Stack>
      </Surface>
    </Stack>
  );
}
