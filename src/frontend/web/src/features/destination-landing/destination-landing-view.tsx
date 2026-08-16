import Link from "next/link";
import {
  Container,
  LtrValue,
  Stack,
  Surface,
  Text,
} from "@/components/ui";
import type { DestinationLandingPageViewModel } from "@/types/pages/destination-landing";

function kindLabel(kind: string, locale: string): string {
  if (locale === "fa") {
    switch (kind) {
      case "Country":
        return "کشور";
      case "Region":
        return "منطقه";
      case "City":
        return "شهر";
      case "Area":
        return "ناحیه";
      default:
        return kind;
    }
  }
  return kind;
}

/**
 * Server-only public Destination landing composition (TC-P04-T009).
 * No Tour/Place/Media projections. No SEO engine.
 */
export function DestinationLandingView({
  vm,
}: {
  vm: DestinationLandingPageViewModel;
}) {
  const locale = vm.locale;

  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          <nav aria-label={locale === "fa" ? "مسیر مقصد" : "Destination path"}>
            <ol className="flex flex-wrap items-center gap-2 text-sm">
              {vm.breadcrumb.map((crumb, index) => {
                const href =
                  crumb.slug != null
                    ? `/${locale}/destinations/${encodeURIComponent(crumb.slug)}`
                    : null;
                return (
                  <li
                    key={`${crumb.code}-${index}`}
                    className="inline-flex items-center gap-2"
                  >
                    {index > 0 ? <span aria-hidden="true">/</span> : null}
                    {href ? (
                      <Link
                        href={href}
                        className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
                      >
                        {crumb.name}
                      </Link>
                    ) : (
                      <span>{crumb.name}</span>
                    )}
                  </li>
                );
              })}
            </ol>
          </nav>

          <Stack gap="sm">
            <Text as="h1" role="heading">
              {vm.name}
            </Text>
            <Text role="muted">
              {kindLabel(vm.kind, locale)} · <LtrValue>{vm.code}</LtrValue>
              {vm.isoCountryCode ? (
                <>
                  {" "}
                  · ISO <LtrValue>{vm.isoCountryCode}</LtrValue>
                </>
              ) : null}
            </Text>
            {vm.description ? <Text>{vm.description}</Text> : null}
          </Stack>

          {vm.latitude != null && vm.longitude != null ? (
            <Surface tone="muted">
              <Text role="caption">
                {locale === "fa" ? "مختصات" : "Coordinates"}:{" "}
                <LtrValue>{String(vm.latitude)}</LtrValue>,{" "}
                <LtrValue>{String(vm.longitude)}</LtrValue>
              </Text>
            </Surface>
          ) : null}

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {locale === "fa" ? "زیرمقصدها" : "Sub-destinations"}
            </Text>
            {vm.children.length === 0 ? (
              <Text role="muted">
                {locale === "fa" ? "موردی ثبت نشده است." : "None listed yet."}
              </Text>
            ) : (
              <ul className="flex flex-col gap-2">
                {vm.children.map((child) => {
                  const href =
                    child.slug != null
                      ? `/${locale}/destinations/${encodeURIComponent(child.slug)}`
                      : null;
                  return (
                    <li
                      key={`${child.code}-${child.kind}`}
                      className="rounded-md border border-border px-3 py-3"
                    >
                      {href ? (
                        <Link
                          href={href}
                          className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
                        >
                          {child.name}
                        </Link>
                      ) : (
                        <span>{child.name}</span>
                      )}
                      <Text role="caption">
                        {kindLabel(child.kind, locale)} ·{" "}
                        <LtrValue>{child.code}</LtrValue>
                      </Text>
                    </li>
                  );
                })}
              </ul>
            )}
          </Stack>

          <Text role="caption">
            {locale === "fa"
              ? "صفحهٔ عمومی مقصد — بدون ایندکس موتور جستجو در P04 (R3)."
              : "Public Destination page — not search-indexable in P04 (R3)."}
          </Text>
        </Stack>
      </Container>
    </div>
  );
}
