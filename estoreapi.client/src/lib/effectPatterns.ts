/** Animated background patterns */
export interface FrameContext {
  width: number;
  height: number;
  colour: string;
  sizeMultiplier: number;
  /** Monotonically increasing time accumulator (~0.01/frame), for things like twinkle. */
  t: number;
  /** Delta-time multiplier
   * Multiply every per-frame motion step by this so motion speed is frame-rate independent. */
  dt: number;
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

/** Particles */

interface FallingParticle {
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

interface TwinklingParticle {
    x: number,  // starting x pos
    y: number,  // starting y pos
    vx: number, // x speed
    vy: number, // y speed
    size: number,   // particle size (radius)
    phase: number,  // twinkle starting phase (brightening & dimming)
}

interface SpinningCube {
    x: number,      // screen position (stationary)
    y: number,
    size: number,   // cube half-extent
    ax: number,     // rotation angle around the X axis
    ay: number,     // rotation angle around the Y axis
    vax: number,    // angular velocity around X
    vay: number,    // angular velocity around Y
}

// Particle count scaled to canvas area, so density stays constant across screen sizes. 
// areaPerParticle = 2,073,600 / desired number of particles at 1080p
function particleDensityCount(width: number, height: number, areaPerParticle: number, max: number, min = 0): number {
    // one particle per areaPerParticle px^2
    const particles = Math.round((width * height) / areaPerParticle); 
    return Math.max(min, Math.min(max, particles));
}

/** Patterns */

function makeLeaf(width: number): FallingParticle {
    return {
        // spawn point: top of the page
        x: Math.random() * width,
        y: -10 - Math.random() * 40,
        size: 12 + Math.random() * 8,
        rot: Math.random() * Math.PI * 2,   // start angle between 0 and 2π
        vr: (Math.random() - 0.5) * 0.03,
        vy: 0.3 + Math.random() * 0.6,
        drift: Math.random() * Math.PI * 2, // drift starts between 0 and 2π
        driftSpeed: 0.004 + Math.random() * 0.01,
        wobble: 0.2 + Math.random() * 0.2,
    }
}

export function CreateFallingLeaves(): PatternEffect<FallingParticle> {
    function seed(width: number, height: number): FallingParticle[] {
        const leaves: FallingParticle[] = [];
        // around 32 leaves on 1080p
        const count = particleDensityCount(width, height, 64800, 64, 8);
        for (let i = 0; i < count; i++) {
            const l = makeLeaf(width);
            // initial spread
            l.y = Math.random() * height;
            leaves.push(l);
        }
        return leaves;
    }

    function frame(
        ctx: CanvasRenderingContext2D,
        particles: FallingParticle[],
        { width, height, colour, sizeMultiplier, dt }: FrameContext,
    ) {
        for (const p of particles) {
            p.y += p.vy * dt;             // fall: add fall speed to vertical position
            p.rot += p.vr * dt;           // spin: add spin speed to current angle
            p.drift += p.driftSpeed * dt; // advance the drift phase for this frame
            p.x += Math.sin(p.drift) * p.wobble * dt;    // horizontal movement = drift

            // recycle once off the bottom edge
            if (p.y > height + 15) Object.assign(p, makeLeaf(width));

            const s = p.size * sizeMultiplier; // final draw size
            ctx.save();
            ctx.translate(p.x, p.y); // move the origin to the leaf's position
            ctx.rotate(p.rot);       // rotate the canvas to the leaf's current angle
            ctx.fillStyle = colour;

            // draw leaf
            ctx.globalAlpha = 0.4;
            ctx.beginPath();
            // one lobe = a narrow, long ellipse fanned out from the base with angle
            const lobe = (angle: number) => {
                const cx = Math.sin(angle) * s * 0.5;
                const cy = -Math.cos(angle) * s * 0.5;
                ctx.ellipse(cx, cy, s * 0.22, s * 0.55, angle, 0, Math.PI * 2);
            };
            ctx.beginPath();
            lobe(-Math.PI / 4); // left lobe, 45 deg
            lobe(0);            // centre lobe, straight up
            lobe(Math.PI / 4);  // right lobe, 45 deg mirrored
            ctx.fill();

            ctx.restore();
        }
    }

    return { seed, frame };
}

function makePetal(width: number, height: number, fallAngle: number): FallingParticle {
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
    vy: 0.4 + Math.random() * 0.8,
    drift: Math.random() * Math.PI * 2, // drift starts between 0 and 2π
    driftSpeed: 0.005 + Math.random() * 0.01,
    wobble: 0.2 + Math.random() * 0.4,
  };
}

export function CreateFallingPetals(): PatternEffect<FallingParticle> {
    const fallAngle = Math.PI / 4; // (π/4 = 45 deg toward the right)

    function seed(width: number, height: number): FallingParticle[] {
        const petals: FallingParticle[] = [];
        // around 48 petals on 1080p
        const count = particleDensityCount(width, height, 43200, 96, 12);
        for (let i = 0; i < count; i++) {
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
        particles: FallingParticle[],
        { width, height, colour, sizeMultiplier, dt }: FrameContext,
    ) {
        for (const p of particles) {
            p.y += p.vy * dt;             // fall: add fall speed to vertical position
            p.rot += p.vr * dt;           // spin: add spin speed to current angle
            p.drift += p.driftSpeed * dt; // advance the drift phase for this frame
            // horizontal movement = the fall angle applied to this petal's fall speed + drift
            p.x += (p.vy * Math.tan(fallAngle) + Math.sin(p.drift) * p.wobble) * dt;

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

function makeStar(width: number, height: number): TwinklingParticle {
    return {
        // spawn point: anywhere on the screen
        x:  Math.random() * width,
        y:  Math.random() * height,
        vx: (Math.random() - 0.5) * 0.25,
        vy: (Math.random() - 0.5) * 0.25,
        size: 1.6 + Math.random() * 1.6,
        phase: Math.random() * Math.PI * 2, // twinkle starts between least & full brightness
    }
}

export function CreateNetworkPattern(): PatternEffect<TwinklingParticle> {
    const CONNECT_DISTANCE = 120;

    function seed(width: number, height: number): TwinklingParticle[] {
        const stars: TwinklingParticle[] = [];
        // around 80 stars on 1080p
        const count = particleDensityCount(width, height, 25920, 160, 20);
        for (let i = 0; i < count; i++) {
            stars.push(makeStar(width, height));
        }
        return stars;
    }

    function frame(
        ctx: CanvasRenderingContext2D,
        particles: TwinklingParticle[],
        { width, height, colour, t, dt }: FrameContext,
    ) {
        for (const p of particles) {
            // move particles
            p.x += p.vx * dt;
            p.y += p.vy * dt;
            // screen wrapping
            if (p.x < 0) p.x = width;
            if (p.x > width) p.x = 0;
            if (p.y < 0) p.y = height;
            if (p.y > height) p.y = 0;
        }

        // draw connection lines
        ctx.strokeStyle = colour;
        ctx.lineWidth = 1;
        // O(n^2) match every star
        for (let i = 0; i < particles.length; i++) {
            for (let j = i + 1; j < particles.length; j++) {
                // get distance between each star
                const dx = particles[i].x - particles[j].x;
                const dy = particles[i].y - particles[j].y;
                const distance = Math.sqrt(dx * dx + dy * dy);

                // draw a line if distance is closer than CONNECT_DISTANCE
                if (distance < CONNECT_DISTANCE) {
                    // have line get stronger if stars are closer
                    ctx.globalAlpha = (1 - distance / CONNECT_DISTANCE) * 0.15;
                    ctx.beginPath();
                    ctx.moveTo(particles[i].x, particles[i].y);
                    ctx.lineTo(particles[j].x, particles[j].y);
                    ctx.stroke();
                }
            }
        }

        // draw the stars
        ctx.fillStyle = colour;
        for (const p of particles) {
            // twinkle brightness oscillator between 0 and 1
            const twinkle = 0.5 + 0.5 * Math.sin(t * 2 + p.phase);
            // twinkle strength between 100% alpha and 50% alpha
            ctx.globalAlpha = 0.5 + twinkle * 0.5;

            ctx.beginPath();
            ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
            ctx.fill();
        }
    }
    
    // reseed stars on resize
    return { seed, frame, onResize: (_particles, width, height) => seed(width, height) }
}

function makeCube(width: number, height: number): SpinningCube {
    return {
        // stationary: placed once on spawn, never moves
        x: Math.random() * width,
        y: Math.random() * height,
        size: 10 + Math.random() * 10,
        ax: Math.random() * Math.PI * 2,    // random start orientation
        ay: Math.random() * Math.PI * 2,
        vax: (Math.random() - 0.5) * 0.02,  // slow spin, either direction
        vay: (Math.random() - 0.5) * 0.02,
    }
}

export function CreateSpinningCubes(): PatternEffect<SpinningCube> {
    const FOV = 8; // perspective depth: higher = flatter, lower = stronger 3D

    // unit cube: 8 corners and the 12 edges that connect them
    const CUBE_VERTS: [number, number, number][] = [
        [-1, -1, -1], [1, -1, -1], [1, 1, -1], [-1, 1, -1], // front face (z = -1)
        [-1, -1,  1], [1, -1,  1], [1, 1,  1], [-1, 1,  1], // back face  (z = +1)
    ];

    const CUBE_EDGES: [number, number][] = [
        [0, 1], [1, 2], [2, 3], [3, 0], // front square
        [4, 5], [5, 6], [6, 7], [7, 4], // back square
        [0, 4], [1, 5], [2, 6], [3, 7], // connectors
    ];


    function seed(width: number, height: number): SpinningCube[] {
        const cubes: SpinningCube[] = [];
        // around 24 cubes on 1080p
        const count = particleDensityCount(width, height, 86400, 48, 6);
        for (let i = 0; i < count; i++) {
            cubes.push(makeCube(width, height));
        }
        return cubes;
    }

    function frame(
        ctx: CanvasRenderingContext2D,
        particles: SpinningCube[],
        { colour, sizeMultiplier, dt }: FrameContext,
    ) {
        for (const p of particles) {
            // spin in place around two axes
            p.ax += p.vax * dt;
            p.ay += p.vay * dt;

            const cosx = Math.cos(p.ax), sinx = Math.sin(p.ax);
            const cosy = Math.cos(p.ay), siny = Math.sin(p.ay);
            const s = p.size * sizeMultiplier;

            // rotate each vertex (X then Y) and project 3D -> 2D
            const pts = CUBE_VERTS.map(([vx, vy, vz]) => {
                const y = vy * cosx - vz * sinx;    // rotate around X
                let z   = vy * sinx + vz * cosx;
                const x = vx * cosy + z * siny;     // rotate around Y
                z       = -vx * siny + z * cosy;
                const scale = FOV / (FOV + z);      // perspective: nearer (smaller z) = larger
                return [x * s * scale, y * s * scale] as const;
            });

            ctx.save();
            ctx.translate(p.x, p.y); // set cube position

            // draw cubes
            ctx.globalAlpha = 0.75;
            ctx.strokeStyle = colour;
            ctx.lineWidth = 1.2;
            ctx.beginPath();
            for (const [a, b] of CUBE_EDGES) {
                ctx.moveTo(pts[a][0], pts[a][1]);
                ctx.lineTo(pts[b][0], pts[b][1]);
            }
            ctx.stroke();
            ctx.restore();
        }
    }

    // reseed on resize: positions are area-dependent and the cubes never move otherwise
    return { seed, frame, onResize: (_particles, width, height) => seed(width, height) }
}