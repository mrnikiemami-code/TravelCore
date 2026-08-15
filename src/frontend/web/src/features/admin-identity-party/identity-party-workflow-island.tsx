"use client";

import { useId, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import {
  createIdentityAccountAction,
  createPersonPartyAction,
  linkPartyAction,
  searchPartiesAction,
  unlinkPartyAction,
} from "@/features/admin-identity-party/actions";
import { getIdentityPartyWorkflowCopy } from "@/features/admin-identity-party/copy";
import type {
  AccountStatusView,
  PartySummaryView,
} from "@/features/admin-identity-party/types";

export type IdentityPartyWorkflowIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

export function IdentityPartyWorkflowIsland({
  locale,
  apiConfigured,
}: IdentityPartyWorkflowIslandProps) {
  const copy = getIdentityPartyWorkflowCopy(locale);
  const formId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [account, setAccount] = useState<AccountStatusView | null>(null);
  const [selectedParty, setSelectedParty] = useState<PartySummaryView | null>(
    null,
  );
  const [candidates, setCandidates] = useState<PartySummaryView[]>([]);

  function run(action: () => Promise<void>) {
    setError(null);
    startTransition(async () => {
      try {
        await action();
      } catch {
        setError(copy.errorGeneric);
      }
    });
  }

  if (!apiConfigured) {
    return (
      <Surface tone="muted">
        <Text role="muted">{copy.apiMissing}</Text>
      </Surface>
    );
  }

  return (
    <Stack gap="lg">
      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.stepAccount}
          </Text>
          <form
            id={`${formId}-account`}
            className="flex flex-col gap-3"
            onSubmit={(e) => {
              e.preventDefault();
              const fd = new FormData(e.currentTarget);
              const email = String(fd.get("email") ?? "");
              const password = String(fd.get("password") ?? "");
              run(async () => {
                const result = await createIdentityAccountAction({
                  email,
                  password,
                });
                if (!result.ok) {
                  setError(
                    result.status === 401
                      ? copy.unauthorizedBody
                      : copy.errorGeneric,
                  );
                  return;
                }
                setAccount(result.account);
              });
            }}
          >
            <label className="flex flex-col gap-1 text-sm">
              <span>{copy.emailLabel}</span>
              <input
                name="email"
                type="email"
                required
                autoComplete="username"
                className="min-h-touch rounded-md border border-border bg-background px-3"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span>{copy.passwordLabel}</span>
              <input
                name="password"
                type="password"
                required
                minLength={8}
                autoComplete="new-password"
                className="min-h-touch rounded-md border border-border bg-background px-3"
              />
            </label>
            <button
              type="submit"
              disabled={pending}
              className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
            >
              {copy.createAccount}
            </button>
          </form>
          {account ? (
            <Text role="muted">
              {copy.accountCreated}: <LtrValue>{account.email}</LtrValue>
            </Text>
          ) : null}
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.stepParty}
          </Text>
          <form
            className="flex flex-col gap-3 sm:flex-row sm:items-end"
            onSubmit={(e) => {
              e.preventDefault();
              const q = String(new FormData(e.currentTarget).get("q") ?? "");
              run(async () => {
                const result = await searchPartiesAction(q);
                if (!result.ok) {
                  setError(copy.errorGeneric);
                  return;
                }
                setCandidates(result.items);
              });
            }}
          >
            <label className="flex min-w-0 flex-1 flex-col gap-1 text-sm">
              <span>{copy.partySearchLabel}</span>
              <input
                name="q"
                type="search"
                className="min-h-touch rounded-md border border-border bg-background px-3"
              />
            </label>
            <button
              type="submit"
              disabled={pending}
              className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
            >
              {copy.partySearch}
            </button>
          </form>

          {candidates.length === 0 ? (
            <Text role="caption">{copy.noResults}</Text>
          ) : (
            <ul className="flex flex-col gap-2">
              {candidates.map((p) => (
                <li
                  key={p.id}
                  className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border px-3 py-2"
                >
                  <div className="min-w-0">
                    <Text as="p">{p.displayName}</Text>
                    <Text role="caption">
                      {p.kind}
                      {p.primaryEmail ? (
                        <>
                          {" · "}
                          <LtrValue>{p.primaryEmail}</LtrValue>
                        </>
                      ) : null}
                    </Text>
                  </div>
                  <button
                    type="button"
                    disabled={pending}
                    className="min-h-touch rounded-md border border-border px-3"
                    onClick={() => setSelectedParty(p)}
                  >
                    {copy.selectParty}
                  </button>
                </li>
              ))}
            </ul>
          )}

          <form
            className="flex flex-col gap-3 border-t border-border pt-3"
            onSubmit={(e) => {
              e.preventDefault();
              const fd = new FormData(e.currentTarget);
              const displayName = String(fd.get("displayName") ?? "");
              const givenName = String(fd.get("givenName") ?? displayName);
              const familyName = String(fd.get("familyName") ?? "-");
              run(async () => {
                const result = await createPersonPartyAction({
                  displayName,
                  givenName,
                  familyName,
                });
                if (!result.ok) {
                  setError(copy.errorGeneric);
                  return;
                }
                setSelectedParty(result.party);
                setCandidates((prev) => [result.party, ...prev]);
              });
            }}
          >
            <Text as="h3" role="label">
              {copy.createParty}
            </Text>
            <label className="flex flex-col gap-1 text-sm">
              <span>{copy.partyDisplayName}</span>
              <input
                name="displayName"
                required
                className="min-h-touch rounded-md border border-border bg-background px-3"
              />
            </label>
            <button
              type="submit"
              disabled={pending}
              className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
            >
              {copy.createParty}
            </button>
          </form>
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.stepLink}
          </Text>
          <Text role="muted">{copy.inspectTitle}</Text>
          <Text role="caption">
            Account:{" "}
            {account ? <LtrValue>{account.email}</LtrValue> : "—"}
          </Text>
          <Text role="caption">
            Party: {selectedParty ? selectedParty.displayName : "—"}
          </Text>
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              disabled={pending || !account || !selectedParty}
              className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
              onClick={() => {
                if (!account || !selectedParty) return;
                run(async () => {
                  const mode = account.associatedPartyId ? "replace" : "link";
                  const result = await linkPartyAction({
                    accountId: account.id,
                    partyId: selectedParty.id,
                    mode,
                  });
                  if (!result.ok) {
                    setError(copy.errorGeneric);
                    return;
                  }
                  setAccount(result.account);
                });
              }}
            >
              {account?.associatedPartyId ? copy.replaceParty : copy.linkParty}
            </button>
            <button
              type="button"
              disabled={pending || !account?.associatedPartyId}
              className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
              onClick={() => {
                if (!account) return;
                run(async () => {
                  const result = await unlinkPartyAction(account.id);
                  if (!result.ok) {
                    setError(copy.errorGeneric);
                    return;
                  }
                  setAccount(result.account);
                });
              }}
            >
              {copy.unlinkParty}
            </button>
          </div>
        </Stack>
      </Surface>

      {error ? (
        <Surface tone="muted">
          <Text role="muted">{error}</Text>
        </Surface>
      ) : null}
    </Stack>
  );
}
