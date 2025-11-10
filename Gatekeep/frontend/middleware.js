import { NextResponse } from 'next/server'

export function middleware(request) {
  const { nextUrl } = request

  return NextResponse.next()
}

export const config = {
  matcher: ['/']
}
