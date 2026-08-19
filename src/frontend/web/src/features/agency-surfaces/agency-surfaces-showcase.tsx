import Link from "next/link";
import { Container, Stack, Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

/** UIVAL-T015 Agency surfaces validation showcase. */
export function AgencySurfacesShowcase({ locale }: { locale: AppLocale }) {
  const copy =
    locale === "fa"
      ? {
          title: "سطوح Agency Marketplace",
          body: "پروفایل · Offer · انتشار — بدون Booking/Payment/Commission",
          note: "Published Offer ≠ SEO Indexed",
          panel: "پنل آژانس production",
        }
      : {
          title: "Agency Marketplace surfaces",
          body: "Profile · Offer · publish workflow — no Booking/Payment/Commission",
          note: "Published Offer ≠ SEO Indexed",
          panel: "Production agency panel",
        };

  return (
    <div className="py-8">
      <Container width="content">
        <Stack gap="lg">
          <Surface>
            <Stack gap="sm">
              <Text as="h1" role="heading">{copy.title}</Text>
              <Text role="muted">{copy.body}</Text>
              <Text role="caption">{copy.note}</Text>
            </Stack>
          </Surface>
          <Link
            href={`/${locale}/agency`}
            className="min-h-touch inline-flex items-center rounded-md border border-border px-4 py-3 underline-offset-2 hover:underline"
          >
            {copy.panel}
          </Link>
          <Text role="caption">/api/agency-marketplace/profiles · /api/agency-marketplace/offers</Text>
        </Stack>
      </Container>
    </div>
  );
}
