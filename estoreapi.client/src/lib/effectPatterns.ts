/** Animated background patterns */
export interface FrameContext {
  width: number;
  height: number;
  colour: string;
  sizeMultiplier: number;
  /** Monotonically increasing time accumulator (~0.01/frame), for things like twinkle. */
  t: number;
}

export interface PatternEffect<TParticle = unknown> {
  /** Seed particles for the given canvas size. Called once on first mount. */
  seed(width: number, height: number): TParticle[];
  /** Advance + draw one frame. Mutate particles in place. */
  frame(ctx: CanvasRenderingContext2D, particles: TParticle[], frame: FrameContext): void;
  /**
   * Called when the container resizes (after the initial mount). 
   * Return a replacement particle array to fully reseed.
   * Omit to keep particles as-is through the resize.
   */
  onResize?(particles: TParticle[], width: number, height: number): TParticle[];
}

// for theme mapping
export type EffectFactory = () => PatternEffect;

/** Autumn leaves */

export function CreateFallingLeaves(): PatternEffect {
    // TODO:
    // straight falling leaves with some drift, heavier than petals
    return {
        seed: () => [],
        frame: () => {},
    };
}

/** Sakura petals */

interface Petal {
    x: number,  // starting x pos
    y: number,  // starting y pos
    size: number,   // particle size
    rot: number,    // starting rotation
    vr: number, // rotation speed
    vy: number, // fall speed
    drift: number,  // starting drift position
    driftSpeed: number, // how fast particle goes through drift cycle (per frame)
    wobble: number,  // drift amplitude
}

function makePetal(width: number, height: number, fallAngle: number): Petal {
  // Petals drift right as they fall, so they spawn through the top and left edges
  // get proportional share of left vs top spawns
  const leftInflow = height * Math.tan(fallAngle);
  const topInflow = width;
  const fromLeft = Math.random() < leftInflow / (leftInflow + topInflow); // proportional left/top split
  return {
    // spawn point: down the left edge if fromLeft, else across the top (offscreen above)
    // spawn particles offscreen with negative values
    x: fromLeft ? -10 : Math.random() * width,
    y: fromLeft ? Math.random() * height : -10 - Math.random() * 40,
    size: 8 + Math.random() * 8,
    rot: Math.random() * Math.PI * 2,   // start angle between 0 and 2π
    vr: (Math.random() - 0.5) * 0.03,
    vy: 0.3 + Math.random() * 0.6,
    drift: Math.random() * Math.PI * 2, // drift starts between 0 and 2π
    driftSpeed: 0.005 + Math.random() * 0.01,
    wobble: 0.2 + Math.random() * 0.4,
  };
}

export function CreateFallingPetals(): PatternEffect<Petal> {
    const PETAL_COUNT = 32;
    const fallAngle = Math.PI / 4; // (π/4 = 45 deg toward the right)

    function seed(width: number, height: number): Petal[] {
        const petals: Petal[] = [];
        for (let i = 0; i < PETAL_COUNT; i++) {
            const p = makePetal(width, height, fallAngle);
            // override spawn: spread across full width/height on first paint
            p.x = Math.random() * width;
            p.y = Math.random() * height;
            petals.push(p);
        }

        return petals;
    }

    function frame(
        ctx: CanvasRenderingContext2D,
        particles: Petal[],
        { width, height, colour, sizeMultiplier }: FrameContext,
    ) {
        for (const p of particles) {
            p.y += p.vy;             // fall: add fall speed to vertical position
            p.rot += p.vr;           // spin: add spin speed to current angle
            p.drift += p.driftSpeed; // advance the drift phase for this frame
            // horizontal movement = the fall angle applied to this petal's fall speed + drift
            p.x += p.vy * Math.tan(fallAngle) + Math.sin(p.drift) * p.wobble;

            // recycle once off the bottom or right edge (+16px offscreen margin)
            if (p.y > height + 16 || p.x > width + 16) Object.assign(p, makePetal(width, height, fallAngle));

            const s = p.size * sizeMultiplier; // final draw size
            ctx.save();
            ctx.translate(p.x, p.y); // move the origin to the petal's position
            ctx.rotate(p.rot);       // rotate the canvas to the petal's current angle
            ctx.fillStyle = colour;

            // two overlapping ellipses make a petal
            ctx.globalAlpha = 0.2;
            ctx.beginPath();
            // lobe 1
            ctx.ellipse(-s * 0.2, 0, s * 0.6, s * 0.3, 0.3, 0, Math.PI * 2);
            ctx.fill();
            ctx.globalAlpha = 0.15;
            ctx.beginPath();
            // lobe 2 mirror
            ctx.ellipse(s * 0.2, 0, s * 0.6, s * 0.3, -0.3, 0, Math.PI * 2);
            ctx.fill();
            ctx.restore();
        }
    }

    return { seed, frame };
}