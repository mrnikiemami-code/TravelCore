import { Container, LtrValue, Stack, Text } from "@/components/ui";
import type { ContentDetailPageViewModel } from "@/types/pages/content-detail";

function kindLabel(kind: string, locale: string): string {
  if (locale === "fa") {
    switch (kind) {
      case "Article":
        return "مقاله";
      case "LandingPage":
        return "صفحه فرود";
      default:
        return kind;
    }
  }
  return kind;
}

/**
 * Server-only public Content detail (TC-P08-T008).
 * Editorial presentation only — no Admin lifecycle / IndexPolicy mutation.
 */
export function ContentDetailView({ vm }: { vm: ContentDetailPageViewModel }) {
  const locale = vm.locale;

  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          <Stack gap="sm">
            <Text as="h1" role="heading">
              {vm.title}
            </Text>
            <Text role="muted">
              {kindLabel(vm.kind, locale)} · <LtrValue>{vm.code}</LtrValue>
            </Text>
            {vm.excerpt ? <Text as="p">{vm.excerpt}</Text> : null}
            {vm.body ? <Text as="p">{vm.body}</Text> : null}
          </Stack>

          {vm.blocks.length > 0 ? (
            <Stack gap="md">
              {vm.blocks.map((block) => {
                if (block.kind === "Heading" && block.text) {
                  return (
                    <Text key={block.id} as="h2" role="heading">
                      {block.text}
                    </Text>
                  );
                }
                if (block.kind === "Paragraph" && block.text) {
                  return (
                    <Text key={block.id} as="p">
                      {block.text}
                    </Text>
                  );
                }
                if (block.kind === "Cta" && block.text && block.href) {
                  return (
                    <Text key={block.id} as="p">
                      <a
                        href={block.href}
                        className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
                      >
                        {block.text}
                      </a>
                    </Text>
                  );
                }
                if (block.text) {
                  return (
                    <Text key={block.id} as="p" role="caption">
                      {block.text}
                    </Text>
                  );
                }
                return null;
              })}
            </Stack>
          ) : null}
        </Stack>
      </Container>
    </div>
  );
}
