"use server";

import { apiGetJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import type {
  PublicMoneyView,
  PublicPriceSummaryView,
} from "./load-tour-detail";

type ApiPublicMoney = {
  amount: number;
  currencyCode: string;
};

type ApiPublicPriceSummary = {
  priceId: string;
  targetType: string;
  targetId: string;
  currency: string;
  components?: Array<{ kind: string; money: ApiPublicMoney }> | null;
  occupancyPrices?: Array<{
    passengerCategory: string;
    occupancyCategory: string;
    money: ApiPublicMoney;
  }> | null;
};

function mapPublicMoney(money: ApiPublicMoney): PublicMoneyView {
  return {
    amount: money.amount,
    currencyCode: money.currencyCode,
  };
}

/**
 * Public Pricing summary for one TourDeparture (P12-R8 · P33-T006).
 * 404 / transport errors → null (honest empty). Never invents money.
 */
export async function loadPublicPriceSummary(
  tourDepartureId: string,
): Promise<PublicPriceSummaryView | null> {
  const id = tourDepartureId.trim();
  if (!id) {
    return null;
  }

  const result = await apiGetJson<ApiPublicPriceSummary>(
    `/api/pricing/public/tour-departures/${encodeURIComponent(id)}`,
    { cache: "no-store" },
  );
  if (!isApiOk(result)) {
    return null;
  }

  const data = result.data;
  return {
    priceId: data.priceId,
    targetType: data.targetType,
    targetId: data.targetId,
    currency: data.currency,
    components: (data.components ?? []).map((c) => ({
      kind: c.kind,
      money: mapPublicMoney(c.money),
    })),
    occupancyPrices: (data.occupancyPrices ?? []).map((row) => ({
      passengerCategory: row.passengerCategory,
      occupancyCategory: row.occupancyCategory,
      money: mapPublicMoney(row.money),
    })),
  };
}

/** Client-callable: fetch Pricing summary for the selected TourDepartureId. */
export async function loadPublicDeparturePriceAction(
  tourDepartureId: string,
): Promise<PublicPriceSummaryView | null> {
  return loadPublicPriceSummary(tourDepartureId);
}
