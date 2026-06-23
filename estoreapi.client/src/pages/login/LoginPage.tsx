import { useEffect } from "react";
import { Button } from "@/components/ui/button";
import { useSearchParams } from "react-router-dom";
import { toast } from '@/components/CustomToast';
import { Card, CardHeader, CardTitle, CardFooter, CardDescription } from "@/components/ui/card";
import { ThemeLogo } from "@/components/ThemeIcon";
import GoogleLogo  from  "../../assets/Google__G__logo.svg";
import { EffectBackground } from "@/components/EffectBackground";

export default function LoginPage() {
    // the server redirects here with ?error=denied when the google account is not whitelisted
    const [searchParams] = useSearchParams();
    const denied = searchParams.get("error") === "denied";

    useEffect(() => {
        if (denied) {
            toast.error("This account does not have access. Contact your administrator.");
        }
    }, [denied]);

    // redirect to google signin
    const signIn = () => {
        window.location.href = "/api/auth/login";
    };

    return (
        <EffectBackground>
            <div className="relative flex min-h-screen justify-center items-center p-4 z-1">
                <Card className="w-full max-w-md border border-border">
                    <CardHeader className="pb-8">
                        <CardTitle className="flex items-center gap-2 flex-wrap"><ThemeLogo className="h-4"/>Management Console</CardTitle>
                        <CardDescription>Authorised accounts only</CardDescription>
                    </CardHeader>
                    <CardFooter>
                        <Button className="mx-auto" variant="outline" onClick={signIn}><img src={GoogleLogo} className="h-4" /> Sign in with Google</Button>
                    </CardFooter>
                </Card>
            </div>
        </EffectBackground>
    );
}
