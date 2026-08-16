"use client";

import { useId, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import {
  loadDestinationSeoPostureAction,
  publishDestinationSeoRouteAction,
  setDestinationIndexPolicyAction,
} from "@/features/admin-destination-seo/actions";
import { getDestinationSeoPostureCopy } from "@/features/admin-destination-seo/copy";
import type { SeoDestinationPostureView } from "@/features/admin-destination-seo/types";

export type DestinationSeoPosturePanelProps = {
  locale: AppLocale;
  destinationId: string | null;
  defaultSlug?: string | null;
};

export function DestinationSeoPosturePanel({
  locale,
  destinationId,
  defaultSlug,
}: DestinationSeoPosturePanelProps) {
  const copy = getDestinationSeoPostureCopy(locale);
  const formId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [postureLocale, setPostureLocale] = useState<"fa" | "en">(
    locale === "en" ? "en" : "fa",
  );
  const [posture, setPosture] = useState<SeoDestinationPostureView | null>(null);

  function mapAuthError(status?: number) {
    if (status === 401 || status === 403) return copy.unauthorizedBody;
    return copy.errorGeneric;
  }

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

  async function refresh(destinationIdValue: string, localeCode: string) {
    const result = await loadDestinationSeoPostureAction({
      destinationId: destinationIdValue,
      locale: localeCode,
    });
    if (!result.ok) {
      setError(mapAuthError(result.status));
      setPosture(null);
      return;
    }
    setPosture(result.posture);
  }

  if (!destinationId) {
    return (
      <Surface>
        <Text role="muted">{copy.needFocus}</Text>
      </Surface>
    );
  }

  return (
    <Surface>
      <Stack gap="sm">
        <Text as="h2" role="heading">
          {copy.stepTitle}
        </Text>
        <Text role="muted">{copy.intro}</Text>
        <Text role="caption">{copy.publishNote}</Text>
        {error ? <Text role="muted">{error}</Text> : null}

        <div className="flex flex-wrap items-end gap-3">
          <label className="flex flex-col gap-1 text-sm">
            <span>{copy.localeLabel}</span>
            <select
              value={postureLocale}
              onChange={(e) => setPostureLocale(e.target.value as "fa" | "en")}
              className="min-h-touch rounded-md border border-border bg-background px-3"
            >
              <option value="fa">fa</option>
              <option value="en">en</option>
            </select>
          </label>
          <button
            type="button"
            disabled={pending}
            className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
            onClick={() =>
              run(async () => {
                await refresh(destinationId, postureLocale);
              })
            }
          >
            {copy.refresh}
          </button>
        </div>

        {posture ? (
          <Stack gap="sm">
            <Text as="h3" role="heading">
              {copy.routesLabel}
            </Text>
            {posture.routes.length === 0 ? (
              <Text role="muted">{copy.noRoutes}</Text>
            ) : (
              <ul className="text-sm">
                {posture.routes.map((r) => (
                  <li key={r.id}>
                    <LtrValue>
                      /{r.locale}/{r.path}
                    </LtrValue>
                  </li>
                ))}
              </ul>
            )}

            <Text as="h3" role="heading">
              {copy.configuredLabel}
            </Text>
            {posture.configuredPolicy ? (
              <Text role="muted">
                <LtrValue>
                  {posture.configuredPolicy.indexDirective},{" "}
                  {posture.configuredPolicy.followDirective}
                </LtrValue>
              </Text>
            ) : (
              <Text role="muted">{copy.missingPolicy}</Text>
            )}

            <Text as="h3" role="heading">
              {copy.effectiveLabel}
            </Text>
            {posture.effectiveIndexability ? (
              <Stack gap="sm">
                <Text role="muted">
                  {copy.robotsLabel}:{" "}
                  <LtrValue>
                    {posture.effectiveIndexability.robotsDirective}
                  </LtrValue>
                </Text>
                <Text role="caption">
                  {posture.effectiveIndexability.isIndexable
                    ? copy.indexableYes
                    : copy.indexableNo}
                </Text>
              </Stack>
            ) : (
              <Text role="muted">{copy.noRoutes}</Text>
            )}

            <Text role="caption">{posture.notes}</Text>
          </Stack>
        ) : null}

        <form
          id={`${formId}-policy`}
          className="flex flex-col gap-3"
          onSubmit={(e) => {
            e.preventDefault();
            const fd = new FormData(e.currentTarget);
            const indexDirective = String(fd.get("indexDirective") ?? "NoIndex");
            const followDirective = String(fd.get("followDirective") ?? "Follow");
            run(async () => {
              const result = await setDestinationIndexPolicyAction({
                destinationId,
                locale: postureLocale,
                indexDirective,
                followDirective,
              });
              if (!result.ok) {
                setError(mapAuthError(result.status));
                return;
              }
              await refresh(destinationId, postureLocale);
            });
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span>{copy.indexDirectiveLabel}</span>
            <select
              name="indexDirective"
              defaultValue={posture?.configuredPolicy?.indexDirective ?? "NoIndex"}
              className="min-h-touch rounded-md border border-border bg-background px-3"
            >
              <option value="NoIndex">NoIndex</option>
              <option value="Index">Index</option>
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span>{copy.followDirectiveLabel}</span>
            <select
              name="followDirective"
              defaultValue={posture?.configuredPolicy?.followDirective ?? "Follow"}
              className="min-h-touch rounded-md border border-border bg-background px-3"
            >
              <option value="Follow">Follow</option>
              <option value="NoFollow">NoFollow</option>
            </select>
          </label>
          <button
            type="submit"
            disabled={pending}
            className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
          >
            {copy.savePolicy}
          </button>
        </form>

        <form
          id={`${formId}-publish`}
          className="flex flex-col gap-3"
          onSubmit={(e) => {
            e.preventDefault();
            const fd = new FormData(e.currentTarget);
            const slug = String(fd.get("slug") ?? "").trim();
            run(async () => {
              const result = await publishDestinationSeoRouteAction({
                destinationId,
                locale: postureLocale,
                slug,
              });
              if (!result.ok) {
                setError(mapAuthError(result.status));
                return;
              }
              await refresh(destinationId, postureLocale);
            });
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span>{copy.slugForPublish}</span>
            <input
              name="slug"
              required
              defaultValue={defaultSlug ?? ""}
              className="min-h-touch rounded-md border border-border bg-background px-3"
            />
          </label>
          <button
            type="submit"
            disabled={pending}
            className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
          >
            {copy.republish}
          </button>
        </form>
      </Stack>
    </Surface>
  );
}
