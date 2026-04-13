"use client";

import { PostCard, Post } from "./PostCard";

const DUMMY_POSTS: Post[] = [
  {
    id: "1",
    profileImage: "https://api.dicebear.com/7.x/avataaars/svg?seed=john",
    name: "John Builder",
    handle: "johnbuilds",
    timeAgo: "2h",
    text: "Just rented the DeWalt 20V MAX Drill from Shelton Tool-Hire. Absolutely fantastic for my renovation project! 🔧\n\nThe equipment performance is top-notch, and the customer service was excellent. Highly recommend for any DIY enthusiasts!",
    image: "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&h=400&fit=crop",
    commentCount: 24,
    retweetCount: 12,
    likeCount: 156,
    viewCount: 2400,
    liked: false,
    bookmarked: false,
  },
  {
    id: "2",
    profileImage: "https://api.dicebear.com/7.x/avataaars/svg?seed=sarah",
    name: "Sarah Green",
    handle: "sarahgardens",
    timeAgo: "5h",
    text: "The landscaping equipment from @SheltonToolHire made my garden transformation so much easier! 🌿\n\n⭐ Equipment Performance: 5/5\n⭐ Customer Service: 5/5\n⭐ After Sales Support: 5/5\n\nWill definitely be renting again for my next project.",
    commentCount: 45,
    retweetCount: 28,
    likeCount: 312,
    viewCount: 5100,
    liked: true,
    bookmarked: true,
  },
  {
    id: "3",
    profileImage: "https://api.dicebear.com/7.x/avataaars/svg?seed=mike",
    name: "Mike's Plumbing Services",
    handle: "mikeplumbing",
    timeAgo: "1d",
    text: "As a professional plumber, I rely on quality tools. Shelton Tool-Hire's plumbing equipment rental has been a game-changer for my business.\n\nTheir technical support staff really knows their stuff - helped me pick the right pipe cutter for a tricky job. Great prices too! 💰",
    commentCount: 18,
    retweetCount: 8,
    likeCount: 89,
    viewCount: 1200,
    liked: false,
    bookmarked: false,
  },
  {
    id: "4",
    profileImage: "https://api.dicebear.com/7.x/avataaars/svg?seed=emma",
    name: "Emma Decorates",
    handle: "emmadecorates",
    timeAgo: "2d",
    text: "Weekend home makeover complete! 🎨✨\n\nRented the paint sprayer and sander from Shelton Tool-Hire. The calculator on their website made it so easy to figure out the rental costs for my 3-day project.\n\nEquipment was clean, well-maintained, and the hourly rates are very competitive.",
    image: "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=600&h=400&fit=crop",
    commentCount: 67,
    retweetCount: 34,
    likeCount: 423,
    viewCount: 8900,
    liked: true,
    bookmarked: false,
  },
];

export function FeedPage() {
  return (
    <div className="min-h-screen bg-black">
      <div className="max-w-[600px] mx-auto px-4 py-6">
        {/* Header */}
        <header className="sticky top-0 z-10 bg-black/80 backdrop-blur-md border-b border-light-gray/10 -mx-4 px-4 py-4 mb-4">
          <h1 className="text-xl font-bold text-white">Tool Reviews</h1>
          <p className="text-gray text-sm mt-1">Shelton Tool-Hire Community</p>
        </header>

        {/* Feed */}
        <div className="flex flex-col gap-4">
          {DUMMY_POSTS.map((post) => (
            <PostCard key={post.id} post={post} />
          ))}
        </div>
      </div>
    </div>
  );
}
