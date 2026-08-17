export type TourDepartureSummaryView = {
  id: string;
  tourProductId: string;
  status: string;
  startDate: string | null;
  endDate: string | null;
  timeZoneId: string | null;
  minimumPax: number | null;
  maximumPax: number | null;
  createdAt: string;
  updatedAt: string;
};

export type TourDepartureDetailView = TourDepartureSummaryView;
