import { LtrValue, Text } from "@/components/ui";
import type { TourDetailPageViewModel } from "./load-tour-detail";

export function TourDetailView({ vm }: { vm: TourDetailPageViewModel }) {
  return (
    <article>
      <Text as="h1" role="heading">
        {vm.name}
      </Text>
      <Text role="caption">
        {vm.kind} · <LtrValue>{vm.code}</LtrValue> ·{" "}
        <LtrValue>{vm.slug}</LtrValue>
      </Text>
      {vm.description ? <Text as="p">{vm.description}</Text> : null}
    </article>
  );
}
