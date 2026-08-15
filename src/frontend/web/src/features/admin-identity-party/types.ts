/** Presentation-only read shapes for Identity↔Party Admin workflow (not domain authority). */

export type AccountStatusView = {
  id: string;
  email: string;
  status: string;
  associatedPartyId: string | null;
};

export type PartySummaryView = {
  id: string;
  kind: string;
  displayName: string;
  status: string;
  primaryEmail: string | null;
};

export type IdentityPartyWorkflowView = {
  account: AccountStatusView | null;
  linkedParty: PartySummaryView | null;
  candidates: PartySummaryView[];
};
