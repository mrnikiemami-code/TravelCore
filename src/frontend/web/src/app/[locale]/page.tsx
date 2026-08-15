import { notFound } from "next/navigation";
import { AdminShell, PublicShell } from "@/components/shell";
import {
  BidiText,
  Container,
  Inline,
  LtrValue,
  MixedCurrencyPrice,
  MoneyText,
  MediaImage,
  Stack,
  Surface,
  Text,
} from "@/components/ui";
import {
  getHtmlDir,
  getHtmlLang,
  isAppLocale,
  type AppLocale,
} from "@/lib/i18n";
import type { MixedCurrencyPriceView, MoneyView } from "@/types/money";

/** Fixture — authoritative values supplied as if from a read model (not calculated here). */
const SAMPLE_USD: MoneyView = { amount: "1290", currencyCode: "USD" };
const SAMPLE_IRR: MoneyView = { amount: "119900000", currencyCode: "IRR" };
const SAMPLE_MIXED: MixedCurrencyPriceView = {
  components: [
    { ...SAMPLE_USD, purpose: "PackagePrice" },
    { ...SAMPLE_IRR, purpose: "LocalCharge" },
  ],
};

/**
 * Minimal locale home — foundation smoke under PublicShell (not a product page).
 */
export default async function LocaleHomePage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const lang = getHtmlLang(locale);
  const dir = getHtmlDir(locale);
  const irrDisplayUnit = locale === "fa" ? "Toman" : "IRR";

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          TravelCore
        </Text>
      }
      footer={
        <Text role="caption">Public shell footer slot — structure only</Text>
      }
    >
      <div className="py-8">
        <Container width="content">
          <Stack gap="lg">
            <Surface>
              <Stack gap="sm">
                <Text as="h1" role="heading">
                  TravelCore
                </Text>
                <Text role="muted">
                  Locale foundation — <LtrValue>{locale}</LtrValue>
                </Text>
                <Text role="caption">
                  document: lang=<LtrValue>{lang}</LtrValue> dir=
                  <LtrValue>{dir}</LtrValue>
                </Text>
              </Stack>
            </Surface>

            <Surface tone="muted">
              <Stack gap="md">
                <Text as="h2" role="title">
                  Bidi isolation smoke
                </Text>
                <Text role="body">
                  مرجع رزرو: <LtrValue>TC-REF-88421</LtrValue>
                </Text>
                <Text role="body">
                  مسیر: <LtrValue>IKA → IST</LtrValue>
                </Text>
                <Text role="body">
                  مهمان: <BidiText dir="rtl">علی رضایی</BidiText>
                  {" · "}
                  <LtrValue>guest@example.com</LtrValue>
                </Text>
                <Inline gap="sm">
                  <span className="inline-flex min-h-touch items-center rounded-md bg-primary px-4 text-label text-primary-foreground">
                    primary
                  </span>
                  <span className="inline-flex min-h-touch items-center rounded-md bg-surface px-4 text-label text-foreground ring-2 ring-focus">
                    focus
                  </span>
                </Inline>
              </Stack>
            </Surface>

            <Surface>
              <Stack gap="md">
                <Text as="h2" role="title">
                  Money presentation smoke
                </Text>
                <Text role="caption">
                  Locale formats digits only; currency comes from supplied Money
                  (irrDisplayUnit=<LtrValue>{irrDisplayUnit}</LtrValue>{" "}
                  explicit).
                </Text>
                <div>
                  <Text role="label">USD on this locale</Text>
                  <div className="mt-1">
                    <MoneyText money={SAMPLE_USD} locale={locale} />
                  </div>
                </div>
                <div>
                  <Text role="label">IRR canonical → display unit</Text>
                  <div className="mt-1">
                    <MoneyText
                      money={SAMPLE_IRR}
                      locale={locale}
                      irrDisplayUnit={irrDisplayUnit}
                    />
                  </div>
                </div>
                <div>
                  <Text role="label">MixedCurrencyPrice (no FX / no sum)</Text>
                  <div className="mt-2">
                    <MixedCurrencyPrice
                      price={SAMPLE_MIXED}
                      locale={locale}
                      irrDisplayUnit={irrDisplayUnit}
                    />
                  </div>
                </div>
              </Stack>
            </Surface>

            <Surface>
              <Stack gap="md">
                <Text as="h2" role="title">
                  Image foundation smoke
                </Text>
                <Text role="caption">
                  Local static fixture via MediaImage (next/image). Not a Tour
                  gallery. priority opt-in only; alt from presentation model.
                </Text>
                <MediaImage
                  src="/media/foundation-sample.png"
                  alt="Sample foundation media for TravelCore image contract"
                  aspectRatio="16 / 9"
                  sizes="(max-width: 768px) 100vw, 640px"
                />
              </Stack>
            </Surface>

            <Surface>
              <Stack gap="md">
                <Text as="h2" role="title">
                  Admin shell composition smoke
                </Text>
                <Text role="caption">
                  Navigation slot may host job-based IA from T010 later — no
                  domain-mirrored menu tree here.
                </Text>
                <div className="overflow-hidden rounded-lg border border-border">
                  <AdminShell
                    embedded
                    header={
                      <Text as="p" role="label">
                        Admin workspace chrome
                      </Text>
                    }
                    navigation={
                      <Text role="caption">
                        Navigation slot (job-based model — T010; items undecided)
                      </Text>
                    }
                    actions={
                      <span className="inline-flex min-h-touch items-center rounded-md border border-border px-3 text-label">
                        actions slot
                      </span>
                    }
                  >
                    <div className="p-4">
                      <Text role="body">
                        Workspace content region — layout only
                      </Text>
                    </div>
                  </AdminShell>
                </div>
              </Stack>
            </Surface>
          </Stack>
        </Container>
      </div>
    </PublicShell>
  );
}
