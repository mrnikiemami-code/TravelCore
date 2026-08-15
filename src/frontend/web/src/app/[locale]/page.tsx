import { notFound } from "next/navigation";
import {
  BidiText,
  Container,
  Inline,
  LtrValue,
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

/**
 * Minimal locale home — routing + token + primitive/bidi smoke (not a product page).
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

  return (
    <main className="flex flex-1 flex-col bg-background py-8 text-foreground">
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
                مهمان:{" "}
                <BidiText dir="rtl">علی رضایی</BidiText>
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
        </Stack>
      </Container>
    </main>
  );
}
