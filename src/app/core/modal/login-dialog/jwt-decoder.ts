
// const headerIndex = 0;
const payloadIndex = 1;
// const signatureIndex = 2;

interface JwtPayload {
    email: string;
    exp: number;
    iat: number;
    roles: string[];
}

export function jwtDecoder(token: string): JwtPayload {
    const base64payload = token.split('.')[payloadIndex];
    const base64 = base64payload.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    
    return JSON.parse(jsonPayload);
}