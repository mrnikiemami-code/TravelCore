import { asPageViewModel } from "@/lib/api/read-models";
import type { ForeignTourDetailPageViewModel } from "@/types/pages/foreign-tour-detail";

/**
 * Deterministic FA fixture — illustrative / non-production (docs/pages/01).
 * Bidi-sensitive LTR islands: IKA, IST, TK875, USD.
 */
export const foreignTourDetailFaFixture: ForeignTourDetailPageViewModel =
  asPageViewModel({
    locale: "fa",
    product: {
      productKey: "fixture-istanbul-package",
      title: "تور استانبول — پرواز مستقیم و هتل‌های انتخابی",
      summary:
        "پکیج خارجی نمونه برای Walking Skeleton؛ دادهٔ نمایشی و غیرعملیاتی.",
    },
    destination: {
      name: "استانبول",
      countryName: "ترکیه",
    },
    duration: {
      nights: 4,
      days: 5,
      label: "۴ شب / ۵ روز",
    },
    commercialStatus: "active",
    hero: {
      src: "/media/foundation-sample.png",
      alt: "نمای نمونه از مقصد استانبول برای صفحهٔ جزئیات تور",
      aspectRatio: "16 / 9",
      sizes: "(max-width: 768px) 100vw, 960px",
      priority: true,
    },
    departures: [
      {
        departureKey: "dep-2026-09-12",
        departureDateLabel: "۱۲ شهریور ۱۴۰۵",
        availabilityLabel: "ظرفیت محدود",
        selected: true,
      },
      {
        departureKey: "dep-2026-09-19",
        departureDateLabel: "۱۹ شهریور ۱۴۰۵",
        availabilityLabel: "موجود",
      },
    ],
    flights: [
      {
        originAirportCode: "IKA",
        destinationAirportCode: "IST",
        carrierCode: "TK",
        flightNumber: "TK875",
        departureLocalLabel: "۱۰:۳۰ (تهران)",
        arrivalLocalLabel: "۱۲:۴۵ (استانبول)",
        cabinClassLabel: "اکونومی",
        baggageLabel: "۳۰ کیلوگرم",
      },
    ],
    hotelOptions: [
      {
        optionKey: "hotel-a-bb",
        hotelName: "هتل نمونه آ (۴★)",
        starLabel: "۴★",
        mealPlanLabel: "BB",
        nights: 4,
        occupancyContextLabel: "دو تخته",
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
        summary: "موقعیت مرکزی · صبحانه",
      },
      {
        optionKey: "hotel-b-hb",
        hotelName: "هتل نمونه ب (۵★)",
        starLabel: "۵★",
        mealPlanLabel: "HB",
        nights: 4,
        occupancyContextLabel: "دو تخته",
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
        summary: "نمای شهر · نیم‌پرس",
      },
    ],
    pricingOffers: [
      {
        offerKey: "adult-double",
        passengerCategory: "Adult",
        occupancy: "Double",
        irrDisplayUnit: "Toman",
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
        irrDisplayUnit: "Toman",
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
        irrDisplayUnit: "Toman",
        price: {
          components: [
            { amount: "990", currencyCode: "USD", purpose: "PackagePrice" },
          ],
        },
      },
    ],
    services: {
      included: [
        "پرواز رفت‌وبرگشت",
        "اقامت هتل طبق گزینه",
        "ترانسفر فرودگاهی",
      ],
      excluded: ["هزینهٔ ویزا", "بیمهٔ مسافرتی", "هزینه‌های شخصی"],
    },
    requirements: [
      "گذرنامه با اعتبار حداقل ۶ ماه",
      "قوانین ورود مقصد ممکن است تغییر کند",
    ],
    policies: [
      "کنسلی طبق شرایط نرخ انتخاب‌شده",
      "قیمت نمایشی Quote نیست و رزرو را تضمین نمی‌کند",
    ],
    itinerarySummary: [
      {
        day: 1,
        title: "ورود به استانبول",
        summary: "ترانسفر و استقرار در هتل.",
      },
      {
        day: 2,
        title: "گشت شهری",
        summary: "بازدید نقاط شاخص طبق برنامهٔ پکیج.",
      },
    ],
    agency: {
      name: "آژانس نمونه TravelCore",
      note: "شناسهٔ تجاری نمایشی — Identity/Party مالکیت جدا دارند.",
    },
    relatedTours: [
      {
        title: "تور آنتالیا نمونه",
        hrefHint: "/fa/tours/antalya-sample",
      },
    ],
    cta: {
      kind: "book",
      label: "ادامه برای رزرو",
      enabled: true,
    },
    seo: {
      title: "تور استانبول | TravelCore",
      description:
        "جزئیات پکیج خارجی نمونه استانبول با پرواز، هتل و قیمت چندارزی.",
    },
  });
