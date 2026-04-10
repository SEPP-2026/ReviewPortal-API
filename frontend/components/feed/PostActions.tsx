"use client";

import { useState } from "react";
import {
  MessageCircle,
  Repeat2,
  Heart,
  BarChart2,
  Bookmark,
  Share,
} from "lucide-react";

interface PostActionsProps {
  commentCount: number;
  retweetCount: number;
  likeCount: number;
  viewCount: number;
  initialLiked?: boolean;
  initialBookmarked?: boolean;
}

export function PostActions({
  commentCount,
  retweetCount,
  likeCount,
  viewCount,
  initialLiked = false,
  initialBookmarked = false,
}: PostActionsProps) {
  const [liked, setLiked] = useState(initialLiked);
  const [bookmarked, setBookmarked] = useState(initialBookmarked);
  const [currentLikeCount, setCurrentLikeCount] = useState(likeCount);

  const handleLike = () => {
    setLiked(!liked);
    setCurrentLikeCount(liked ? currentLikeCount - 1 : currentLikeCount + 1);
  };

  const formatCount = (count: number): string => {
    if (count >= 1000000) {
      return (count / 1000000).toFixed(1).replace(/\.0$/, "") + "M";
    }
    if (count >= 1000) {
      return (count / 1000).toFixed(1).replace(/\.0$/, "") + "K";
    }
    return count.toString();
  };

  return (
    <div className="flex items-center justify-between mt-3 max-w-md">
      {/* Comment */}
      <button className="group flex items-center gap-2 text-gray hover:text-primary transition-colors duration-200 cursor-pointer">
        <div className="p-2 rounded-full group-hover:bg-primary/10 transition-colors duration-200">
          <MessageCircle className="w-[18px] h-[18px]" />
        </div>
        <span className="text-sm">{formatCount(commentCount)}</span>
      </button>

      {/* Retweet */}
      <button className="group flex items-center gap-2 text-gray hover:text-green-500 transition-colors duration-200 cursor-pointer">
        <div className="p-2 rounded-full group-hover:bg-green-500/10 transition-colors duration-200">
          <Repeat2 className="w-[18px] h-[18px]" />
        </div>
        <span className="text-sm">{formatCount(retweetCount)}</span>
      </button>

      {/* Like */}
      <button
        onClick={handleLike}
        className={`group flex items-center gap-2 transition-colors duration-200 cursor-pointer ${
          liked ? "text-primary" : "text-gray hover:text-primary"
        }`}
      >
        <div
          className={`p-2 rounded-full transition-colors duration-200 ${
            liked ? "bg-primary/10" : "group-hover:bg-primary/10"
          }`}
        >
          <Heart
            className={`w-[18px] h-[18px] ${liked ? "fill-primary" : ""}`}
          />
        </div>
        <span className={`text-sm ${liked ? "text-primary" : ""}`}>
          {formatCount(currentLikeCount)}
        </span>
      </button>

      {/* Views */}
      <button className="group flex items-center gap-2 text-gray hover:text-primary transition-colors duration-200 cursor-pointer">
        <div className="p-2 rounded-full group-hover:bg-primary/10 transition-colors duration-200">
          <BarChart2 className="w-[18px] h-[18px]" />
        </div>
        <span className="text-sm">{formatCount(viewCount)}</span>
      </button>

      {/* Bookmark & Share */}
      <div className="flex items-center gap-1">
        <button
          onClick={() => setBookmarked(!bookmarked)}
          className={`group p-2 rounded-full transition-colors duration-200 cursor-pointer ${
            bookmarked
              ? "text-primary bg-primary/10"
              : "text-gray hover:text-primary hover:bg-primary/10"
          }`}
        >
          <Bookmark
            className={`w-[18px] h-[18px] ${bookmarked ? "fill-primary" : ""}`}
          />
        </button>
        <button className="group p-2 rounded-full text-gray hover:text-primary hover:bg-primary/10 transition-colors duration-200 cursor-pointer">
          <Share className="w-[18px] h-[18px]" />
        </button>
      </div>
    </div>
  );
}
