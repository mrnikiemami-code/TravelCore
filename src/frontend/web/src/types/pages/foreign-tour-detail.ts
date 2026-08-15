import type { AppLocale } from "@/lib/i18n";
import type { PageViewModel } from "@/lib/api/read-models";
import type { MediaImagePresentation } from "@/types/media-image";
import type { IrrDisplayUnit, MixedCurrencyPriceView } from "@/types/money";

/**
 * ForeignTourDetailPage — page-specific presentation model (T012).
 * Domain Model ≠ API Contract ≠ Page View Model
 *
 * Composes presentation facts originating from Tour / Destination / Place /
 * Media / Pricing contracts without merging backend ownership.
 *
 * TourProduct ≠ TourDeparture
 * Price ≠ Quote ≠ Booking ≠ Payment
 * PassengerCategory ≠ Occupancy
 */

export type TourCommercialStatus =
  | "active"
  | "no_departure"
  | "expired"
  | "unavailable";

/** Passenger pricing axis — not the same as room occupancy. */
export type PassengerCategoryView =
  | "Adult"
  | "Child"
  | "Infant"
  | (string & {});

/** Room/occupancy axis for package hotel pricing presentation. */
export type OccupancyView =
  | "Single"
  | "Double"
  | "Triple"
  | "ChildWithBed"
  | (string & {});

export type DestinationContextView = {
  name: string;
  countryName?: string;
};

export type DurationView = {
  nights: number;
  days: number;
  /** Localized human label already prepared upstream. */
  label: string;
};

/**
 * Selectable commercial departure sample — NOT the TourProduct identity.
 */
export type TourDepartureSummaryView = {
  /** Opaque presentation key — not a raw FK UX workflow. */
  departureKey: string;
  /** Localized departure date label (calendar/timezone decided upstream). */
  departureDateLabel: string;
  availabilityLabel?: string;
  selected?: boolean;
};

export type FlightSegmentView = {
  originAirportCode: string;
  destinationAirportCode: string;
  carrierCode?: string;
  flightNumber?: string;
  departureLocalLabel?: string;
  arrivalLocalLabel?: string;
  cabinClassLabel?: string;
  baggageLabel?: string;
};

export type HotelOptionView = {
  optionKey: string;
  hotelName: string;
  starLabel?: string;
  mealPlanLabel?: string;
  nights?: number;
  occupancyContextLabel?: string;
  /** Relative commercial presentation — not a Quote. */
  relativePrice?: MixedCurrencyPriceView;
  summary?: string;
  media?: MediaImagePresentation;
};

export type PricingOfferView = {
  offerKey: string;
  passengerCategory: PassengerCategoryView;
  occupancy: OccupancyView;
  /** Display price components — UI must not FX-convert or invent 0. */
  price: MixedCurrencyPriceView;
  /** Explicit IRR display unit when IRR appears — never implied by locale alone. */
  irrDisplayUnit?: IrrDisplayUnit;
  unavailable?: boolean;
  unavailableReason?: string;
};

export type BookingCtaView = {
  kind: "book" | "contact" | "disabled";
  label: string;
  enabled: boolean;
  reasonDisabled?: string;
};

export type SeoTextView = {
  title: string;
  description: string;
};

export type ForeignTourDetailFields = {
  locale: AppLocale;
  /** TourProduct presentation identity (page root). */
  product: {
    productKey: string;
    title: string;
    summary?: string;
  };
  destination: DestinationContextView;
  duration: DurationView;
  commercialStatus: TourCommercialStatus;
  hero: MediaImagePresentation;
  departures: TourDepartureSummaryView[];
  flights: FlightSegmentView[];
  hotelOptions: HotelOptionView[];
  pricingOffers: PricingOfferView[];
  services: {
    included: string[];
    excluded: string[];
  };
  requirements: string[];
  policies: string[];
  itinerarySummary: Array<{
    day: number;
    title: string;
    summary: string;
  }>;
  agency?: {
    name: string;
    note?: string;
  };
  relatedTours: Array<{
    title: string;
    /** Presentation href hint for later routing — not a live booking link. */
    hrefHint: string;
  }>;
  cta: BookingCtaView;
  seo: SeoTextView;
};

export type ForeignTourDetailPageViewModel =
  PageViewModel<ForeignTourDetailFields>;
