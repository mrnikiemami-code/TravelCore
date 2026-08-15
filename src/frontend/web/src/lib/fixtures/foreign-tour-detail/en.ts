import { asPageViewModel } from "@/lib/api/read-models";
import type { ForeignTourDetailPageViewModel } from "@/types/pages/foreign-tour-detail";

/**
 * Deterministic EN fixture — illustrative / non-production.
 * Must NOT silently reuse FA copy as published EN content.
 */
export const foreignTourDetailEnFixture: ForeignTourDetailPageViewModel =
  asPageViewModel({
    locale: "en",
    product: {
      productKey: "fixture-istanbul-package",
      title: "Istanbul Package — Direct Flight & Select Hotels",
      summary:
        "Illustrative foreign package for the Walking Skeleton; non-operational fixture data.",
    },
    destination: {
      name: "Istanbul",
      countryName: "Turkey",
    },
    duration: {
      nights: 4,
      days: 5,
      label: "4 nights / 5 days",
    },
    commercialStatus: "active",
    hero: {
      src: "/media/foundation-sample.png",
      alt: "Sample Istanbul destination imagery for tour detail foundation",
      aspectRatio: "16 / 9",
      sizes: "(max-width: 768px) 100vw, 960px",
      priority: true,
    },
    departures: [
      {
        departureKey: "dep-2026-09-12",
        departureDateLabel: "12 Sep 2026",
        availabilityLabel: "Limited",
        selected: true,
      },
      {
        departureKey: "dep-2026-09-19",
        departureDateLabel: "19 Sep 2026",
        availabilityLabel: "Available",
      },
    ],
    flights: [
      {
        originAirportCode: "IKA",
        destinationAirportCode: "IST",
        carrierCode: "TK",
        flightNumber: "TK875",
        departureLocalLabel: "10:30 (Tehran local)",
        arrivalLocalLabel: "12:45 (Istanbul local)",
        cabinClassLabel: "Economy",
        baggageLabel: "30 kg",
      },
    ],
    hotelOptions: [
      {
        optionKey: "hotel-a-bb",
        hotelName: "Sample Hotel A (4★)",
        starLabel: "4★",
        mealPlanLabel: "BB",
        nights: 4,
        occupancyContextLabel: "Double",
        relativePrice: {
          components: [
            { amount: "1290", currencyCode: "USD", purpose: "PackagePrice" },
            {
              amount: "119900000",
              currencyCode: "IRR",
              purpose: "LocalCharge",
            },
          ],
        },
        summary: "Central location · breakfast",
      },
      {
        optionKey: "hotel-b-hb",
        hotelName: "Sample Hotel B (5★)",
        starLabel: "5★",
        mealPlanLabel: "HB",
        nights: 4,
        occupancyContextLabel: "Double",
        relativePrice: {
          components: [
            { amount: "1490", currencyCode: "USD", purpose: "PackagePrice" },
            {
              amount: "129900000",
              currencyCode: "IRR",
              purpose: "LocalCharge",
            },
          ],
        },
        summary: "City view · half board",
      },
    ],
    pricingOffers: [
      {
        offerKey: "adult-double",
        passengerCategory: "Adult",
        occupancy: "Double",
        irrDisplayUnit: "IRR",
        price: {
          components: [
            { amount: "1290", currencyCode: "USD", purpose: "PackagePrice" },
            {
              amount: "119900000",
              currencyCode: "IRR",
              purpose: "LocalCharge",
            },
          ],
        },
      },
      {
        offerKey: "adult-single",
        passengerCategory: "Adult",
        occupancy: "Single",
        irrDisplayUnit: "IRR",
        price: {
          components: [
            { amount: "1590", currencyCode: "USD", purpose: "PackagePrice" },
            {
              amount: "139900000",
              currencyCode: "IRR",
              purpose: "LocalCharge",
            },
          ],
        },
      },
      {
        offerKey: "child-with-bed",
        passengerCategory: "Child",
        occupancy: "ChildWithBed",
        irrDisplayUnit: "IRR",
        price: {
          components: [
            { amount: "990", currencyCode: "USD", purpose: "PackagePrice" },
          ],
        },
      },
    ],
    services: {
      included: [
        "Round-trip flights",
        "Hotel stay per selected option",
        "Airport transfers",
      ],
      excluded: ["Visa fees", "Travel insurance", "Personal expenses"],
    },
    requirements: [
      "Passport valid at least 6 months",
      "Entry rules may change without notice",
    ],
    policies: [
      "Cancellation follows the selected rate conditions",
      "Displayed price is not a Quote and does not guarantee booking",
    ],
    itinerarySummary: [
      {
        day: 1,
        title: "Arrival in Istanbul",
        summary: "Transfer and hotel check-in.",
      },
      {
        day: 2,
        title: "City sightseeing",
        summary: "Highlights per package plan.",
      },
    ],
    agency: {
      name: "TravelCore Sample Agency",
      note: "Illustrative seller label — Identity/Party ownership stays separate.",
    },
    relatedTours: [
      {
        title: "Antalya sample package",
        hrefHint: "/en/tours/antalya-sample",
      },
    ],
    cta: {
      kind: "book",
      label: "Continue to booking",
      enabled: true,
    },
    seo: {
      title: "Istanbul Package | TravelCore",
      description:
        "Sample foreign package detail with flights, hotels, and mixed-currency price presentation.",
    },
  });
