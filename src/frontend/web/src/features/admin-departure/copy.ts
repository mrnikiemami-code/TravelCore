import type { AppLocale } from "@/lib/i18n";

export type AdminDepartureWorkflowCopy = {
  pageTitle: string;
  pageIntro: string;
  navLabel: string;
  backToHub: string;
  toursLink: string;
  hubCta: string;
  stepCreate: string;
  stepBrowse: string;
  stepInspect: string;
  tourProductIdLabel: string;
  createAction: string;
  productFilterLabel: string;
  statusFilterLabel: string;
  statusAll: string;
  refreshList: string;
  takeLabel: string;
  noDepartures: string;
  selectDeparture: string;
  selectedTitle: string;
  statusLabel: string;
  saveStatus: string;
  scheduleHeading: string;
  startDateLabel: string;
  endDateLabel: string;
  timeZoneLabel: string;
  saveSchedule: string;
  capacityHeading: string;
  minPaxLabel: string;
  maxPaxLabel: string;
  saveCapacity: string;
  apiMissing: string;
  busy: string;
  errorPrefix: string;
};

const fa: AdminDepartureWorkflowCopy = {
  pageTitle: "مدیریت Departure تور",
  pageIntro:
    "ایجاد و ویرایش TourDeparture · برنامه · ظرفیت · وضعیت اجرا. بدون موتور رزرو/قیمت/پرداخت.",
  navLabel: "Departureها",
  backToHub: "بازگشت به کاتالوگ",
  toursLink: "محصولات تور",
  hubCta: "مدیریت Departure",
  stepCreate: "ایجاد Departure",
  stepBrowse: "فهرست",
  stepInspect: "جزئیات و ویرایش",
  tourProductIdLabel: "شناسه TourProduct",
  createAction: "ایجاد",
  productFilterLabel: "فیلتر محصول",
  statusFilterLabel: "فیلتر وضعیت",
  statusAll: "همه",
  refreshList: "بروزرسانی فهرست",
  takeLabel: "تعداد",
  noDepartures: "Departureی یافت نشد.",
  selectDeparture: "انتخاب",
  selectedTitle: "Departure انتخاب‌شده",
  statusLabel: "وضعیت اجرا",
  saveStatus: "ذخیره وضعیت",
  scheduleHeading: "برنامه سفر",
  startDateLabel: "تاریخ شروع (yyyy-MM-dd)",
  endDateLabel: "تاریخ پایان (yyyy-MM-dd)",
  timeZoneLabel: "منطقه زمانی IANA",
  saveSchedule: "ذخیره برنامه",
  capacityHeading: "ظرفیت برنامه‌ای",
  minPaxLabel: "حداقل نفر",
  maxPaxLabel: "حداکثر نفر",
  saveCapacity: "ذخیره ظرفیت",
  apiMissing: "آدرس API تنظیم نشده است.",
  busy: "در حال انجام…",
  errorPrefix: "خطا:",
};

const en: AdminDepartureWorkflowCopy = {
  pageTitle: "Tour Departure admin",
  pageIntro:
    "Create and edit TourDeparture · schedule · capacity · lifecycle. No reservation/price/pay engines.",
  navLabel: "Departures",
  backToHub: "Back to catalog",
  toursLink: "Tour products",
  hubCta: "Manage departures",
  stepCreate: "Create departure",
  stepBrowse: "Browse",
  stepInspect: "Inspect & edit",
  tourProductIdLabel: "TourProduct id",
  createAction: "Create",
  productFilterLabel: "Product filter",
  statusFilterLabel: "Status filter",
  statusAll: "All",
  refreshList: "Refresh list",
  takeLabel: "Take",
  noDepartures: "No departures found.",
  selectDeparture: "Select",
  selectedTitle: "Selected departure",
  statusLabel: "Lifecycle status",
  saveStatus: "Save status",
  scheduleHeading: "Travel schedule",
  startDateLabel: "Start date (yyyy-MM-dd)",
  endDateLabel: "End date (yyyy-MM-dd)",
  timeZoneLabel: "IANA time zone",
  saveSchedule: "Save schedule",
  capacityHeading: "Planned capacity",
  minPaxLabel: "Minimum pax",
  maxPaxLabel: "Maximum pax",
  saveCapacity: "Save capacity",
  apiMissing: "API base URL is not configured.",
  busy: "Working…",
  errorPrefix: "Error:",
};

export function getAdminDepartureWorkflowCopy(
  locale: AppLocale,
): AdminDepartureWorkflowCopy {
  return locale === "en" ? en : fa;
}
