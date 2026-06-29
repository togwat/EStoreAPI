import { useEffect, useState } from "react";

export type AgentMetadata = { 
	modelContextWindow: number | null 
};

/** Fetch the active model's context window for the context usage indicator. */
export const getAgentMetadata = async (): Promise<AgentMetadata> => {
	const res = await fetch("/agent/metadata", {
		headers: { "Content-Type": "application/json" },
	});
	if (!res.ok) throw new Error(`Agent metadata error ${res.status}: ${res.statusText}`);
	return (await res.json()) as AgentMetadata;
};

/** The model context window, fetched once */
export function useModelContextWindow(): number | undefined {
	const [contextWindow, setContextWindow] = useState<number>();
	
	useEffect(() => {
		let cancelled = false;
		getAgentMetadata()
			.then((metadata) => {
				// null (unknown window) leaves it undefined so the indicator stays hidden.
				if (!cancelled && metadata.modelContextWindow !== null) {
					setContextWindow(metadata.modelContextWindow);
				}
			})
			.catch(() => {
				// Indicator stays hidden if metadata can't be fetched.
			});
		return () => {
			cancelled = true;
		};
	}, []);

	return contextWindow;
}