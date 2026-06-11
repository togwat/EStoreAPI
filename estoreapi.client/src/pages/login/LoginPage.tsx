import { useEffect } from "react";
import { Button } from "@/components/ui/button";
import { useSearchParams } from "react-router-dom";
import { toast } from '@/components/CustomToast';

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
        <div className="flex min-h-screen justify-center items-center">
            <Button onClick={signIn}>Sign in with Google</Button>
        </div>
    );
}
