"use client";

import { PostHeader } from "./PostHeader";
import { PostContent } from "./PostContent";
import { PostActions } from "./PostActions";

export interface Post {
  id: string;
  profileImage: string;
  name: string;
  handle: string;
  timeAgo: string;
  text: string;
  image?: string;
  commentCount: number;
  retweetCount: number;
  likeCount: number;
  viewCount: number;
  liked?: boolean;
  bookmarked?: boolean;
}

interface PostCardProps {
  post: Post;
}

export function PostCard({ post }: PostCardProps) {
  return (
    <article className="bg-dark border border-light-gray/10 rounded-xl p-4 hover:bg-dark/80 transition-colors duration-200 cursor-pointer">
      <PostHeader
        profileImage={post.profileImage}
        name={post.name}
        handle={post.handle}
        timeAgo={post.timeAgo}
      />
      <PostContent text={post.text} image={post.image} />
      <PostActions
        commentCount={post.commentCount}
        retweetCount={post.retweetCount}
        likeCount={post.likeCount}
        viewCount={post.viewCount}
        initialLiked={post.liked}
        initialBookmarked={post.bookmarked}
      />
    </article>
  );
}
