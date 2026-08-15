import { NextRequest, NextResponse } from "next/server";

/**
 * Minimal request annotation for Server Components (loading / not-found)
 * that cannot read `[locale]` params. Not used for locale negotiation.
 * Next.js 16 file convention: `proxy` (replaces deprecated `middleware`).
 */
export function proxy(request: NextRequest) {
  const headers = new Headers(request.headers);
  headers.set("x-pathname", request.nextUrl.pathname);
  return NextResponse.next({ request: { headers } });
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
};
