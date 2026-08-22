export type AgencyOfferGovernanceOpsStatus =
  | "Submitted"
  | "Approved"
  | "Rejected"
  | "Suspended"
  | "Retired";

export type AgencyOfferModerationQueueView = {
  offerId: string;
  agencyProfileId: string;
  tourProductId: string;
  titleOverride: string | null;
  highlight: string | null;
  salesChannel: string;
  status: string;
  visibility: string;
  publicationStatus: string;
  createdAt: string;
  updatedAt: string;
  lastDecisionKind: string | null;
  lastDecisionAt: string | null;
  hasGovernanceHistory: boolean;
};
