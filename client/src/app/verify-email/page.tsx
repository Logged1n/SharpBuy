"use client";

import { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { apiClient } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { CheckCircle, XCircle } from "lucide-react";
import Link from "next/link";
import type { ApiError } from "@/types";

export default function VerifyEmailPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState<"verifying" | "success" | "error">(
    "verifying"
  );
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const email = searchParams.get("email");
    const token = searchParams.get("token");

    if (!email || !token) {
      setStatus("error");
      setError("Missing email or verification token");
      return;
    }

    const verifyEmail = async () => {
      try {
        await apiClient.verifyEmail({ email, token });
        setStatus("success");
        // Redirect to login after 3 seconds
        setTimeout(() => router.push("/login"), 3000);
      } catch (err) {
        const apiError = err as ApiError;
        setStatus("error");
        setError(apiError.title || "Verification failed");
      }
    };

    verifyEmail();
  }, [searchParams, router]);

  return (
    <div className="container flex items-center justify-center min-h-[calc(100vh-200px)] py-10">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            {status === "verifying" && "Verifying Email..."}
            {status === "success" && (
              <>
                <CheckCircle className="h-6 w-6 text-green-600" />
                Email Verified!
              </>
            )}
            {status === "error" && (
              <>
                <XCircle className="h-6 w-6 text-destructive" />
                Verification Failed
              </>
            )}
          </CardTitle>
          <CardDescription>
            {status === "verifying" &&
              "Please wait while we verify your email address..."}
            {status === "success" &&
              "Your email has been successfully verified. Redirecting to login..."}
            {status === "error" && error}
          </CardDescription>
        </CardHeader>
        {status === "success" && (
          <CardContent>
            <Link href="/login">
              <Button className="w-full">Go to Login</Button>
            </Link>
          </CardContent>
        )}
        {status === "error" && (
          <CardContent className="space-y-2">
            <Link href="/register">
              <Button className="w-full">Back to Registration</Button>
            </Link>
          </CardContent>
        )}
      </Card>
    </div>
  );
}
