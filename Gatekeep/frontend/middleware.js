import { NextResponse } from 'next/server'

export function middleware(request) {
    const { nextUrl } = request

    // Redirigir /auth/login a /login
    if (nextUrl.pathname === '/auth/login') {
        return NextResponse.redirect(new URL('/login', request.url))
    }

    // Redirigir /auth/register a /register
    if (nextUrl.pathname === '/auth/register') {
        return NextResponse.redirect(new URL('/register', request.url))
    }

    return NextResponse.next()
}

export const config = {
    matcher: ['/((?!api|_next/static|_next/image|favicon.ico).*)']
}