import {
  BidiText,
  Container,
  FieldMessage,
  Inline,
  LtrValue,
  MediaImage,
  MixedCurrencyPrice,
  MoneyText,
  RouteLoadingSkeleton,
  RouteStatePanel,
  Stack,
  Surface,
  Text,
  VisuallyHidden,
} from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import { getHtmlDir, getHtmlLang } from "@/lib/i18n";
import type { MixedCurrencyPriceView, MoneyView } from "@/types/money";

const SAMPLE_USD: MoneyView = { amount: "1290", currencyCode: "USD" };
const SAMPLE_IRR: MoneyView = { amount: "119900000", currencyCode: "IRR" };
const SAMPLE_MIXED: MixedCurrencyPriceView = {
  components: [
    { ...SAMPLE_USD, purpose: "PackagePrice" },
    { ...SAMPLE_IRR, purpose: "LocalCharge" },
  ],
};

type FoundationPrimitivesShowcaseProps = {
  locale: AppLocale;
};

/**
 * UIVAL-T001 — exhaustive foundation primitive composition (Server Component).
 * Validates P02 T003–T011 primitives under FA/EN document direction.
 */
export function FoundationPrimitivesShowcase({
  locale,
}: FoundationPrimitivesShowcaseProps) {
  const lang = getHtmlLang(locale);
  const dir = getHtmlDir(locale);
  const irrDisplayUnit = locale === "fa" ? "Toman" : "IRR";

  const copy =
    locale === "fa"
      ? {
          title: "اعتبارسنجی primitives پایه",
          subtitle: "مسیر dev-only · noindex · بدون منطق کسب‌وکار",
          layout: "چیدمان direction-neutral",
          typography: "نقش‌های typography",
          bidi: "جداسازی bidi",
          money: "نمایش پول",
          a11y: "دسترس‌پذیری پایه",
          media: "پایه تصویر",
          route: "حالت‌های مسیر",
          fieldHelp: "راهنمای فیلد نمونه",
          fieldError: "خطای فیلد نمونه — معنی فقط با رنگ نیست",
          fieldStatus: "وضعیت فیلد نمونه",
          routeTitle: "پنل وضعیت نمونه",
          routeBody: "RouteStatePanel — بدون فرض دامنه",
          loadingLabel: "در حال بارگذاری نمونه",
        }
      : {
          title: "Foundation primitives validation",
          subtitle: "Dev-only route · noindex · no business logic",
          layout: "Direction-neutral layout",
          typography: "Typography roles",
          bidi: "Bidi isolation",
          money: "Money presentation",
          a11y: "Accessibility baseline",
          media: "Image foundation",
          route: "Route states",
          fieldHelp: "Sample field help",
          fieldError: "Sample field error — meaning is not color-only",
          fieldStatus: "Sample field status",
          routeTitle: "Sample status panel",
          routeBody: "RouteStatePanel — no domain assumptions",
          loadingLabel: "Sample loading",
        };

  return (
    <Stack gap="lg">
      <Surface>
        <Stack gap="sm">
          <Text as="h1" role="heading">
            {copy.title}
          </Text>
          <Text role="muted">{copy.subtitle}</Text>
          <Text role="caption">
            document: lang=<LtrValue>{lang}</LtrValue> dir=
            <LtrValue>{dir}</LtrValue>
          </Text>
        </Stack>
      </Surface>

      <Surface tone="muted">
        <Stack gap="md">
          <Text as="h2" role="title">
            {copy.layout}
          </Text>
          <Stack gap="sm">
            <Text role="label">Container narrow</Text>
            <Container width="narrow" className="rounded-md border border-border bg-surface px-4 py-2">
              <Text role="caption">max-w-narrow</Text>
            </Container>
          </Stack>
          <Stack gap="sm">
            <Text role="label">Container content (default)</Text>
            <Container width="content" className="rounded-md border border-border bg-surface px-4 py-2">
              <Text role="caption">max-w-content</Text>
            </Container>
          </Stack>
          <Stack gap="sm">
            <Text role="label">Container wide</Text>
            <Container width="wide" className="rounded-md border border-border bg-surface px-4 py-2">
              <Text role="caption">max-w-wide</Text>
            </Container>
          </Stack>
          <Inline gap="md">
            <span className="inline-flex min-h-touch items-center rounded-md bg-primary px-4 text-label text-primary-foreground">
              primary
            </span>
            <span className="inline-flex min-h-touch items-center rounded-md bg-surface px-4 text-label text-foreground ring-2 ring-focus">
              focus ring
            </span>
          </Inline>
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="md">
          <Text as="h2" role="title">
            {copy.typography}
          </Text>
          <Text role="display">display</Text>
          <Text role="heading">heading</Text>
          <Text role="title">title</Text>
          <Text role="body">body</Text>
          <Text role="label">label</Text>
          <Text role="caption">caption</Text>
          <Text role="muted">muted</Text>
        </Stack>
      </Surface>

      <Surface tone="muted">
        <Stack gap="md">
          <Text as="h2" role="title">
            {copy.bidi}
          </Text>
          <Text role="body">
            <LtrValue>TC-REF-88421</LtrValue>
          </Text>
          <Text role="body">
            <LtrValue>IKA → IST</LtrValue>
          </Text>
          <Text role="body">
            <LtrValue>EK978</LtrValue>
            {" · "}
            <BidiText dir="auto">guest@example.com</BidiText>
          </Text>
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="md">
          <Text as="h2" role="title">
            {copy.money}
          </Text>
          <div>
            <Text role="label">MoneyText USD</Text>
            <div className="mt-1">
              <MoneyText money={SAMPLE_USD} locale={locale} />
            </div>
          </div>
          <div>
            <Text role="label">
              MoneyText IRR → <LtrValue>{irrDisplayUnit}</LtrValue>
            </Text>
            <div className="mt-1">
              <MoneyText
                money={SAMPLE_IRR}
                locale={locale}
                irrDisplayUnit={irrDisplayUnit}
              />
            </div>
          </div>
          <div>
            <Text role="label">MixedCurrencyPrice</Text>
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

      <Surface tone="muted">
        <Stack gap="md">
          <Text as="h2" role="title">
            {copy.a11y}
          </Text>
          <div>
            <label htmlFor="uival-sample-input" className="text-label font-medium">
              <VisuallyHidden>Sample input label for screen readers</VisuallyHidden>
              <Text role="label">Sample control</Text>
            </label>
            <input
              id="uival-sample-input"
              type="text"
              readOnly
              aria-describedby="uival-field-help uival-field-status"
              aria-errormessage="uival-field-error"
              className="mt-1 block min-h-touch w-full max-w-md rounded-md border border-border bg-surface px-3 text-body"
              defaultValue={locale === "fa" ? "نمونه" : "sample"}
            />
            <FieldMessage id="uival-field-help" tone="help">
              {copy.fieldHelp}
            </FieldMessage>
            <FieldMessage id="uival-field-error" tone="error">
              {copy.fieldError}
            </FieldMessage>
            <FieldMessage id="uival-field-status" tone="status">
              {copy.fieldStatus}
            </FieldMessage>
          </div>
        </Stack>
      </Surface>

      <Surface>
        <Stack gap="md">
          <Text as="h2" role="title">
            {copy.media}
          </Text>
          <MediaImage
            src="/media/foundation-sample.png"
            alt={
              locale === "fa"
                ? "نمونه تصویر foundation برای قرارداد MediaImage"
                : "Foundation sample image for MediaImage contract"
            }
            aspectRatio="16 / 9"
            sizes="(max-width: 768px) 100vw, 640px"
          />
        </Stack>
      </Surface>

      <Surface tone="muted">
        <Stack gap="md">
          <Text as="h2" role="title">
            {copy.route}
          </Text>
          <RouteStatePanel title={copy.routeTitle} body={copy.routeBody} />
          <RouteLoadingSkeleton label={copy.loadingLabel} />
        </Stack>
      </Surface>
    </Stack>
  );
}
