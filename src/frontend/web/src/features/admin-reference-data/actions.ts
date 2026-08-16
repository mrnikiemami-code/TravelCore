"use server";

import { cookies } from "next/headers";
import { apiGetJson } from "@/lib/api/client";
import type {
  CountryCatalogView,
  CurrencyCatalogView,
  LocaleCatalogView,
} from "@/features/admin-reference-data/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiCountry = {
  alpha2Code: string;
  alpha3Code: string;
  numericCode?: string | null;
  englishName: string;
};

type ApiCurrency = {
  code: string;
  englishName: string;
  minorUnits: number;
  symbol?: string | null;
};

type ApiLocale = {
  code: string;
  englishName: string;
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

function fail(
  result: { message: string; status?: number },
): { ok: false; message: string; status?: number } {
  return { ok: false, message: result.message, status: result.status };
}

/** Shared ISO country catalog for Destination Admin pickers (no raw UUID UX). */
export async function listCountriesAction(): Promise<
  | { ok: true; items: CountryCatalogView[] }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<ApiCountry[]>("/api/reference-data/countries", {
    headers: await authHeaders(),
    cache: "no-store",
  });
  if (!result.ok) return fail(result);
  return {
    ok: true,
    items: (result.data ?? []).map((c) => ({
      alpha2Code: c.alpha2Code,
      alpha3Code: c.alpha3Code,
      numericCode: c.numericCode ?? null,
      englishName: c.englishName,
    })),
  };
}

export async function listCurrenciesAction(): Promise<
  | { ok: true; items: CurrencyCatalogView[] }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<ApiCurrency[]>(
    "/api/reference-data/currencies",
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return fail(result);
  return {
    ok: true,
    items: (result.data ?? []).map((c) => ({
      code: c.code,
      englishName: c.englishName,
      minorUnits: c.minorUnits,
      symbol: c.symbol ?? null,
    })),
  };
}

export async function listLocalesAction(): Promise<
  | { ok: true; items: LocaleCatalogView[] }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<ApiLocale[]>("/api/reference-data/locales", {
    headers: await authHeaders(),
    cache: "no-store",
  });
  if (!result.ok) return fail(result);
  return {
    ok: true,
    items: (result.data ?? []).map((l) => ({
      code: l.code,
      englishName: l.englishName,
    })),
  };
}
