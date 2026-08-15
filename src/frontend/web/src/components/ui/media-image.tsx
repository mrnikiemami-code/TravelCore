import Image from "next/image";
import type { MediaImagePresentation } from "@/types/media-image";
import { cn } from "@/lib/ui/cn";

export type MediaImageProps = MediaImagePresentation & {
  className?: string;
  /**
   * `fill` — responsive width + aspect-ratio box (default for public hero/cards).
   * `intrinsic` — fixed width/height from the presentation model.
   */
  layout?: "fill" | "intrinsic";
  /**
   * object-fit for the rendered image. Default `cover` for fill layouts.
   * Prefer `contain` when clipping would drop essential content.
   */
  objectFit?: "cover" | "contain";
};

const DEFAULT_ASPECT = "16 / 9";
const DEFAULT_SIZES = "(max-width: 768px) 100vw, (max-width: 1280px) 80vw, 1200px";

/**
 * Generic responsive media presentation primitive (T011).
 *
 * - Server Component (no `"use client"`)
 * - next/image optimization path
 * - Direction-neutral (no RTL/LTR forks; media not mirrored by dir)
 * - Alt semantics are caller-owned (informative vs decorative)
 * - `priority` is opt-in only
 *
 * Not a Tour/Hotel/Destination domain component.
 */
export function MediaImage({
  src,
  alt,
  width,
  height,
  aspectRatio = DEFAULT_ASPECT,
  sizes = DEFAULT_SIZES,
  priority = false,
  className,
  layout = "fill",
  objectFit = "cover",
}: MediaImageProps) {
  const fitClass =
    objectFit === "contain" ? "object-contain" : "object-cover";

  if (layout === "intrinsic") {
    if (width == null || height == null) {
      throw new Error(
        "MediaImage layout=\"intrinsic\" requires width and height on the presentation model.",
      );
    }

    return (
      <Image
        src={src}
        alt={alt}
        width={width}
        height={height}
        sizes={sizes}
        priority={priority}
        className={cn("h-auto max-w-full", fitClass, className)}
      />
    );
  }

  return (
    <div
      className={cn(
        "relative w-full max-w-full overflow-hidden rounded-lg bg-surface-muted",
        className,
      )}
      style={{ aspectRatio }}
    >
      <Image
        src={src}
        alt={alt}
        fill
        sizes={sizes}
        priority={priority}
        className={cn(fitClass)}
      />
    </div>
  );
}
